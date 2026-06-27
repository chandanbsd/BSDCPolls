import { Component, inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatStepperModule } from '@angular/material/stepper';
import {
  BsdcPollsApiClient,
  SurveyAnswerType,
  SurveyChoiceOption,
  SurveyQuestionNode,
  SurveyQuestionTreeDocument,
} from '../../../generated/api';
import { firstValueFrom } from 'rxjs';

/** Stepper-based form for building a new survey with a conditional question tree. */
@Component({
  selector: 'app-survey-builder',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatStepperModule,
  ],
  templateUrl: './survey-builder.component.html',
  styleUrl: './survey-builder.component.scss',
})
export class SurveyBuilderComponent {
  private readonly apiClient = inject(BsdcPollsApiClient);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly answerTypes = Object.values(SurveyAnswerType);
  readonly SurveyAnswerType = SurveyAnswerType;

  readonly metaForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    isPublic: [false],
  });

  readonly questionsForm = this.fb.nonNullable.group({
    questions: this.fb.array<FormGroup>([]),
  });

  isLoading = false;
  error: string | null = null;

  get questions(): FormArray {
    return this.questionsForm.get('questions') as FormArray;
  }

  addQuestion(): void {
    this.questions.push(
      this.fb.nonNullable.group({
        text: ['', [Validators.required, Validators.maxLength(500)]],
        answerType: [SurveyAnswerType.MultipleChoice, Validators.required],
        isRequired: [true],
        choices: this.fb.array([this.newChoiceGroup(), this.newChoiceGroup()]),
      }),
    );
  }

  removeQuestion(index: number): void {
    this.questions.removeAt(index);
  }

  choicesFor(questionIndex: number): FormArray {
    return this.questions.at(questionIndex).get('choices') as FormArray;
  }

  addChoice(questionIndex: number): void {
    this.choicesFor(questionIndex).push(this.newChoiceGroup());
  }

  removeChoice(questionIndex: number, choiceIndex: number): void {
    this.choicesFor(questionIndex).removeAt(choiceIndex);
  }

  async onSubmit(): Promise<void> {
    if (this.metaForm.invalid || this.questionsForm.invalid) return;
    this.isLoading = true;
    this.error = null;
    try {
      const questionTree = this.buildQuestionTree();
      const { title, isPublic } = this.metaForm.getRawValue();
      const survey = await firstValueFrom(this.apiClient.surveys_Create({ title, isPublic, questionTree }));
      await this.router.navigate(['/surveys', survey.surveyUid]);
    } catch (err: unknown) {
      this.error = err instanceof Error ? err.message : 'Failed to create survey.';
    } finally {
      this.isLoading = false;
    }
  }

  private newChoiceGroup(): FormGroup {
    return this.fb.nonNullable.group({
      uid: [crypto.randomUUID()],
      text: ['', [Validators.required, Validators.maxLength(200)]],
    });
  }

  private buildQuestionTree(): SurveyQuestionTreeDocument {
    const nodes: SurveyQuestionNode[] = this.questions.controls.map((ctrl) => {
      const { text, answerType, isRequired, choices } = ctrl.getRawValue() as {
        text: string;
        answerType: SurveyAnswerType;
        isRequired: boolean;
        choices: Array<{ uid: string; text: string }>;
      };
      const choiceOptions: SurveyChoiceOption[] | undefined =
        answerType === SurveyAnswerType.MultipleChoice ? choices.map((c) => ({ uid: c.uid, text: c.text })) : undefined;
      return {
        uid: crypto.randomUUID(),
        text,
        answerType,
        isRequired,
        choices: choiceOptions,
        branches: undefined,
      };
    });
    return { questions: nodes };
  }
}
