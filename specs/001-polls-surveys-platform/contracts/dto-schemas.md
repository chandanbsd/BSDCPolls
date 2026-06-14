# API Contracts: DTO Schemas

**Date**: 2026-06-10
**Location**: All DTOs defined in `BSDCPolls.Contracts` project
**Validation**: Every request DTO has a co-located `AbstractValidator<T>` in `BSDCPolls.Contracts`
**TypeScript generation**: NSwag generates types from BFF OpenAPI spec into `src/app/generated/`

---

## Validation Rules (FluentValidation — source of truth for all validation)

All rules below define the authoritative C# FluentValidation constraints. Angular form validators
are derived from NSwag-generated metadata where expressible in OpenAPI; for complex rules, Angular
implements a custom reactive validator with a `// Mirrors: BSDCPolls.Contracts.<Validator>.<Rule>`
comment.

---

### RegisterRequest

```csharp
// BSDCPolls.Contracts.Validators.RegisterRequestValidator
RuleFor(x => x.Password)
    .NotEmpty()
    .MinimumLength(12)
    .Matches(@"[A-Z]").WithMessage("Must contain at least one uppercase letter.")
    .Matches(@"[a-z]").WithMessage("Must contain at least one lowercase letter.")
    .Matches(@"[0-9]").WithMessage("Must contain at least one digit.")
    .Matches(@"[^a-zA-Z0-9]").WithMessage("Must contain at least one special character.");
```

---

### LoginRequest

```csharp
// BSDCPolls.Contracts.Validators.LoginRequestValidator
RuleFor(x => x.Username).NotEmpty().MaximumLength(60);
RuleFor(x => x.Password).NotEmpty();
```

---

### CreatePollRequest

```csharp
// BSDCPolls.Contracts.Validators.CreatePollRequestValidator
RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
RuleFor(x => x.IsPublic).NotNull();
```

---

### AddPollQuestionRequest

```csharp
// BSDCPolls.Contracts.Validators.AddPollQuestionRequestValidator
RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
RuleFor(x => x.Options)
    .NotNull()
    .Must(opts => opts.Count >= 2).WithMessage("A question must have at least 2 answer options.")
    .Must(opts => opts.Count <= 10).WithMessage("A question may have at most 10 answer options.");
RuleForEach(x => x.Options).ChildRules(opt => {
    opt.RuleFor(o => o.Text).NotEmpty().MaximumLength(200);
    opt.RuleFor(o => o.OrderIndex).GreaterThanOrEqualTo(0);
});
RuleFor(x => x.PushImmediately).NotNull();
```

---

### SubmitPollVoteRequest

```csharp
// BSDCPolls.Contracts.Validators.SubmitPollVoteRequestValidator
RuleFor(x => x.QuestionUid).NotEmpty();
RuleFor(x => x.SelectedOptionUid).NotEmpty();
```

---

### CreateSurveyRequest

```csharp
// BSDCPolls.Contracts.Validators.CreateSurveyRequestValidator
RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
RuleFor(x => x.IsPublic).NotNull();
RuleFor(x => x.QuestionTree).NotNull().SetValidator(new SurveyQuestionTreeDocumentValidator());
```

---

### SurveyQuestionTreeDocumentValidator (nested)

```csharp
// BSDCPolls.Contracts.Validators.SurveyQuestionTreeDocumentValidator
RuleFor(x => x.Questions).NotNull().NotEmpty()
    .WithMessage("A survey must have at least one question.");
RuleForEach(x => x.Questions).SetValidator(new SurveyQuestionNodeValidator());
```

---

### SurveyQuestionNodeValidator (nested, recursive)

```csharp
// BSDCPolls.Contracts.Validators.SurveyQuestionNodeValidator
RuleFor(x => x.Text).NotEmpty().MaximumLength(500);
RuleFor(x => x.AnswerType).IsInEnum();
// MultipleChoice requires at least 2 choices
RuleFor(x => x.Choices)
    .Must((node, choices) => node.AnswerType != SurveyAnswerType.MultipleChoice || (choices != null && choices.Count >= 2))
    .WithMessage("Multiple-choice questions must have at least 2 choices.");
// Non-MultipleChoice questions must not have choices
RuleFor(x => x.Choices)
    .Must((node, choices) => node.AnswerType == SurveyAnswerType.MultipleChoice || choices == null || choices.Count == 0)
    .WithMessage("Non-multiple-choice questions must not define answer choices.");
// DocumentUpload questions must not have branches
RuleFor(x => x.Branches)
    .Must((node, branches) => node.AnswerType != SurveyAnswerType.DocumentUpload || branches == null || branches.Count == 0)
    .WithMessage("Document-upload questions may not have conditional branches.");
// Recursive: validate nested branch questions
RuleForEach(x => x.Branches).ChildRules(branch => {
    branch.RuleFor(b => b.ParentChoiceUid).NotEmpty();
    branch.RuleFor(b => b.Questions).NotNull().NotEmpty();
    branch.RuleForEach(b => b.Questions).SetValidator(new SurveyQuestionNodeValidator());
});
```

*Angular mirrors*: The Angular survey builder form uses custom reactive validators for the
MultipleChoice/choices cross-field rules, marked with:
`// Mirrors: BSDCPolls.Contracts.Validators.SurveyQuestionNodeValidator.ChoicesRequiredForMultipleChoice`

---

### SaveSurveyResponseRequest

```csharp
// BSDCPolls.Contracts.Validators.SaveSurveyResponseRequestValidator
RuleFor(x => x.Answers).NotNull();
RuleForEach(x => x.Answers).ChildRules(ans => {
    ans.RuleFor(a => a.QuestionUid).NotEmpty();
    ans.RuleFor(a => a.AnswerType).IsInEnum();
    // MultipleChoice must supply SelectedChoiceUid
    ans.RuleFor(a => a.SelectedChoiceUid)
       .Must((entry, uid) => entry.AnswerType != SurveyAnswerType.MultipleChoice || uid.HasValue)
       .WithMessage("Multiple-choice answers must include a selected choice.");
    // Text answers must supply TextValue
    ans.RuleFor(a => a.TextValue)
       .Must((entry, text) =>
           (entry.AnswerType != SurveyAnswerType.ShortText && entry.AnswerType != SurveyAnswerType.LongText)
           || !string.IsNullOrWhiteSpace(text))
       .WithMessage("Text answers must include a non-empty text value.");
    // DocumentUpload must supply DocumentUid
    ans.RuleFor(a => a.DocumentUid)
       .Must((entry, uid) => entry.AnswerType != SurveyAnswerType.DocumentUpload || uid.HasValue)
       .WithMessage("Document-upload answers must reference an uploaded document.");
});
```

---

### CreateInvitationRequest

```csharp
// BSDCPolls.Contracts.Validators.CreateInvitationRequestValidator
RuleFor(x => x.TargetUsername).NotEmpty().MaximumLength(60);
```

**Note**: The "exactly one of PollId or SurveyId" constraint is enforced at the BFF controller
level via route design (separate endpoints for poll vs. survey invitations), not in this DTO.

---

### UpdatePrivacySettingsRequest

```csharp
// BSDCPolls.Contracts.Validators.UpdatePrivacySettingsRequestValidator
RuleFor(x => x.ShowPublicContent).NotNull();
RuleFor(x => x.InvitePermission).IsInEnum();
```

---

### AddAllowlistEntryRequest

```csharp
// BSDCPolls.Contracts.Validators.AddAllowlistEntryRequestValidator
RuleFor(x => x.Username).NotEmpty().MaximumLength(60);
// Existence validation (username must map to a real user) is enforced in the BFF service
// layer, not in FluentValidation, because it requires a database call.
```

---

### ChangePollStatusRequest / ChangeSurveyStatusRequest

```csharp
// BSDCPolls.Contracts.Validators.ChangePollStatusRequestValidator
RuleFor(x => x.Status)
    .IsInEnum()
    .Must(s => s == PollStatus.Active || s == PollStatus.Closed)
    .WithMessage("Status must be Active or Closed (Draft is the initial state; use create endpoint).");
```

---

## Key Data Transfer Objects

### Response DTOs (generated TypeScript via NSwag — do not hand-write)

All response DTOs follow `PascalCase` in C# and are serialized as `camelCase` JSON. NSwag
generates TypeScript interfaces with `camelCase` property names to match the JSON.

Selected canonical shapes (for frontend reference; authoritative types come from NSwag output):

```typescript
// Generated: src/app/generated/api.ts (via NSwag)

interface UserProfileResponse {
  userUid: string;
  username: string;
  createdOn: string;
}

interface PollFeedItem {
  pollUid: string;
  title: string;
  isPublic: boolean;
  status: 'Draft' | 'Active' | 'Closed';
  creatorUsername: string;
  questionCount: number;
  createdOn: string;
  invitedAt?: string;
}

interface SurveyQuestionNodeDto {
  uid: string;
  text: string;
  answerType: 'MultipleChoice' | 'ShortText' | 'LongText' | 'DocumentUpload';
  isRequired: boolean;
  choices?: Array<{ uid: string; text: string }>;
  branches?: Array<{
    parentChoiceUid: string;
    questions: SurveyQuestionNodeDto[];
  }>;
}

interface NotificationItem {
  notificationUid: string;
  isRead: boolean;
  createdOn: string;
  invitation: {
    invitationUid: string;
    inviterUsername: string;
    pollUid?: string;
    pollTitle?: string;
    surveyUid?: string;
    surveyTitle?: string;
  };
}

interface ApiErrorResponse {
  traceId: string;
  status: number;
  message: string;
  errors?: Array<{ field?: string; message: string }>;
}
```

> **Rule**: Never define these shapes by hand in Angular code. Import from `src/app/generated/`.
> The NSwag generation pipeline (`npm run generate-api`) regenerates these from the BFF
> OpenAPI spec whenever contracts change.
