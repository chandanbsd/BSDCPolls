import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import {
  BsdcPollsApiClient,
  SurveyDetailResponse,
  SurveyAnswerEntry,
  SurveyAnswerType,
  SurveyQuestionNode,
  SurveyStatus,
} from '../generated/api';
import { firstValueFrom } from 'rxjs';

interface SurveyState {
  survey: SurveyDetailResponse | null;
  /** UIDs of questions currently visible to the respondent based on prior answers. */
  currentQuestionPath: string[];
  answers: Map<string, SurveyAnswerEntry>;
  responseUid: string | null;
  isSubmitting: boolean;
  error: string | null;
}

const initialState: SurveyState = {
  survey: null,
  currentQuestionPath: [],
  answers: new Map(),
  responseUid: null,
  isSubmitting: false,
  error: null,
};

/** Resolves the flat list of visible question UIDs based on the current answer map. */
function resolveQuestionPath(
  questions: SurveyQuestionNode[],
  answers: Map<string, SurveyAnswerEntry>,
): string[] {
  const path: string[] = [];
  for (const node of questions) {
    path.push(node.uid);
    if (node.branches && node.branches.length > 0) {
      const answer = answers.get(node.uid);
      const selectedChoiceUid = answer?.selectedChoiceUid ?? null;
      if (selectedChoiceUid) {
        const branch = node.branches.find((b) => b.parentChoiceUid === selectedChoiceUid);
        if (branch) {
          path.push(...resolveQuestionPath(branch.questions, answers));
        }
      }
    }
  }
  return path;
}

/**
 * NgRx Signal Store for an active survey session.
 * Tracks the respondent's navigation path through conditional question branches.
 */
export const SurveyStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => {
    const apiClient = inject(BsdcPollsApiClient);
    return {
      /** Loads survey details from the BFF. */
      async loadSurvey(surveyUid: string): Promise<void> {
        patchState(store, { error: null });
        try {
          const survey = await firstValueFrom(apiClient.surveys_GetByUid(surveyUid));
          const path = resolveQuestionPath(survey.questionTree.questions, new Map());
          patchState(store, {
            survey,
            currentQuestionPath: path,
            answers: new Map(),
            responseUid: null,
          });
        } catch (err: unknown) {
          patchState(store, { error: err instanceof Error ? err.message : 'Failed to load survey.' });
        }
      },

      /** Records an answer and recomputes the visible question path. */
      setAnswer(questionUid: string, entry: SurveyAnswerEntry): void {
        const survey = store.survey();
        if (!survey) return;
        const newAnswers = new Map(store.answers());
        newAnswers.set(questionUid, entry);
        const path = resolveQuestionPath(survey.questionTree.questions, newAnswers);
        patchState(store, { answers: newAnswers, currentQuestionPath: path });
      },

      /** Saves progress to the BFF without submitting. */
      async saveProgress(surveyUid: string): Promise<void> {
        const answers = Array.from(store.answers().values());
        try {
          const result = await firstValueFrom(
            apiClient.surveys_SaveResponse(surveyUid, { answers, isSubmitting: false }),
          );
          patchState(store, { responseUid: result.responseUid });
        } catch (err: unknown) {
          patchState(store, { error: err instanceof Error ? err.message : 'Failed to save progress.' });
        }
      },

      /** Submits the final response to the BFF. */
      async submit(surveyUid: string): Promise<void> {
        patchState(store, { isSubmitting: true, error: null });
        try {
          const answers = Array.from(store.answers().values());
          const result = await firstValueFrom(
            apiClient.surveys_SaveResponse(surveyUid, { answers, isSubmitting: true }),
          );
          patchState(store, { responseUid: result.responseUid, isSubmitting: false });
        } catch (err: unknown) {
          patchState(store, {
            error: err instanceof Error ? err.message : 'Failed to submit survey.',
            isSubmitting: false,
          });
        }
      },
    };
  }),
);

export type SurveyStore = InstanceType<typeof SurveyStore>;
export { SurveyAnswerType, SurveyStatus };
