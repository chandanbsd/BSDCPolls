import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { SurveyStore, SurveyAnswerType } from '../../../store/survey.store';
import { BsdcPollsApiClient, FileParameter, SurveyQuestionNode } from '../../../generated/api';
import { InviteUserDialogComponent } from '../../../shared/invite-user-dialog/invite-user-dialog.component';
import { firstValueFrom } from 'rxjs';

/** Renders the active survey question for a respondent, navigating conditional branches. */
@Component({
  selector: 'app-survey-respondent',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
  ],
  templateUrl: './survey-respondent.component.html',
})
export class SurveyRespondentComponent implements OnInit {
  readonly surveyStore = inject(SurveyStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly apiClient = inject(BsdcPollsApiClient);
  private readonly dialog = inject(MatDialog);

  readonly SurveyAnswerType = SurveyAnswerType;

  surveyUid = '';
  currentQuestionIndex = 0;
  selectedChoiceUid: string | null = null;
  textValue = '';
  selectedFile: File | null = null;
  uploadError: string | null = null;

  async ngOnInit(): Promise<void> {
    this.surveyUid = this.route.snapshot.paramMap.get('surveyUid') ?? '';
    await this.surveyStore.loadSurvey(this.surveyUid);
  }

  get currentQuestion(): SurveyQuestionNode | null {
    const survey = this.surveyStore.survey();
    const path = this.surveyStore.currentQuestionPath();
    if (!survey || path.length === 0) return null;
    const uid = path[this.currentQuestionIndex];
    return this.findQuestion(survey.questionTree.questions, uid) ?? null;
  }

  get isLastQuestion(): boolean {
    return this.currentQuestionIndex >= this.surveyStore.currentQuestionPath().length - 1;
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
    this.uploadError = null;
  }

  async advanceOrSubmit(): Promise<void> {
    const question = this.currentQuestion;
    if (!question) return;

    if (question.answerType === SurveyAnswerType.DocumentUpload) {
      if (!this.selectedFile) return;
      await this.uploadDocument(question.uid);
    } else {
      this.recordAnswer(question);
    }

    if (this.isLastQuestion) {
      await this.surveyStore.submit(this.surveyUid);
      await this.router.navigate(['/surveys', this.surveyUid, 'results']);
    } else {
      this.currentQuestionIndex++;
      this.resetInputs();
    }
  }

  private recordAnswer(question: SurveyQuestionNode): void {
    if (question.answerType === SurveyAnswerType.MultipleChoice && this.selectedChoiceUid) {
      this.surveyStore.setAnswer(question.uid, {
        questionUid: question.uid,
        answerType: SurveyAnswerType.MultipleChoice,
        selectedChoiceUid: this.selectedChoiceUid,
        textValue: undefined,
        documentUid: undefined,
      });
    } else if (
      (question.answerType === SurveyAnswerType.ShortText || question.answerType === SurveyAnswerType.LongText) &&
      this.textValue.trim()
    ) {
      this.surveyStore.setAnswer(question.uid, {
        questionUid: question.uid,
        answerType: question.answerType,
        selectedChoiceUid: undefined,
        textValue: this.textValue.trim(),
        documentUid: undefined,
      });
    }
  }

  private async uploadDocument(questionUid: string): Promise<void> {
    if (!this.selectedFile) return;
    const responseUid = this.surveyStore.responseUid();
    if (!responseUid) {
      await this.surveyStore.saveProgress(this.surveyUid);
    }
    const currentResponseUid = this.surveyStore.responseUid();
    if (!currentResponseUid) return;

    const fileParam: FileParameter = { data: this.selectedFile, fileName: this.selectedFile.name };

    try {
      const result = await firstValueFrom(
        this.apiClient.surveys_UploadDocument(this.surveyUid, currentResponseUid, fileParam, questionUid),
      );
      this.surveyStore.setAnswer(questionUid, {
        questionUid,
        answerType: SurveyAnswerType.DocumentUpload,
        selectedChoiceUid: undefined,
        textValue: undefined,
        documentUid: result.documentUid,
      });
    } catch {
      this.uploadError = 'Failed to upload document. Please ensure it is a PDF under 10 MB.';
    }
  }

  private findQuestion(questions: SurveyQuestionNode[], uid: string): SurveyQuestionNode | undefined {
    for (const q of questions) {
      if (q.uid === uid) return q;
      if (q.branches) {
        for (const branch of q.branches) {
          const found = this.findQuestion(branch.questions, uid);
          if (found) return found;
        }
      }
    }
    return undefined;
  }

  openInviteDialog(): void {
    this.dialog.open(InviteUserDialogComponent, {
      data: { surveyUid: this.surveyUid },
      width: '400px',
    });
  }

  private resetInputs(): void {
    this.selectedChoiceUid = null;
    this.textValue = '';
    this.selectedFile = null;
    this.uploadError = null;
  }
}
