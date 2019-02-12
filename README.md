# EvaluationSystem
Performance Evaluation Front End
This has several functional areas:

1. for supervisors to create evaluations for their reports, and steward the evals through the process to HR.
2. for employees to view/comment on their evaluations from each supervisor.
3. for HR superusers, which in itself has several functional sub areas:
  1. find and view evaluations, completed or in process
  2. masquerade (impersonate) a supervisor to move the process along when intervention is required
  3. manage flagged evaluations requiring HR review
  4. manage a master repository of job descriptions, as they are needed for other things, as well as being updated as part of the eval process
  5. view reports
  6. manage email reminder messages, this is a front-end to an integration services package that sends reminder/update emails to the supervisor list
 
 There is a post processing solution which is an integration services package (that employs a console program) that moves the evaluations 
 that meet criteria through the final storage and transmittal to the campus HR system.
 The console program makes use of a pdf building package which is not compatible with Integration Services.
  
