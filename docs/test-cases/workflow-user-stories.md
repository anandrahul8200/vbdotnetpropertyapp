# Workflow Module - User Stories

## Module: WFL (Workflow)
## Test Type: User Story Acceptance Criteria

---

### User Story ID: US-WFL-001
**Title**: View My Task Queue
**As a** claims adjuster or underwriter
**I want to** see all tasks assigned to me in one place
**So that** I can prioritize my work and ensure nothing falls through the cracks

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Task queue shows my name in the title | Title = "My Task Queue - {CurrentUserFullName}" |
| 2 | Tasks loaded automatically on form open | LoadTasks() called during Load event |
| 3 | I can filter by module (Policy, Claims, etc.) | cboFilter with module options |
| 4 | I can filter by priority (High, Normal, Low) | cboPriority with priority options |
| 5 | Task count is shown | lblTaskCount displays "N pending tasks" |
| 6 | I can refresh the list | Refresh button calls LoadTasks() |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-001 | Task Queue View and Filter |
| UI-WFL-001 | frmTaskQueue controls |
| UT-WFL-001 | LoadTasks method |
| SP-WFL-002 | usp_Task_GetByUser |

---

### User Story ID: US-WFL-002
**Title**: Complete a Task
**As a** claims adjuster
**I want to** mark tasks as completed when I finish them
**So that** my queue reflects only outstanding work

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I must select a task before completing | No action if no row selected |
| 2 | Clicking Complete marks the task done | "Task marked complete." confirmation |
| 3 | Task list refreshes after completion | LoadTasks() called to update display |
| 4 | CompletedBy recorded | My username stored as CompletedBy |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-002 | Task Completion |
| UT-WFL-002 | btnComplete_Click |
| SP-WFL-003 | usp_Task_Complete |

---

### User Story ID: US-WFL-003
**Title**: Reassign a Task
**As a** team manager
**I want to** reassign tasks from one user to another
**So that** I can balance workload across the team

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I must select a task before reassigning | No action if no row selected |
| 2 | Reassignment dialog allows selecting new assignee | Dialog with user selection [ASSUMPTION] |
| 3 | Optional reason for reassignment | @Reason parameter accepted |
| 4 | Task moves to new assignee's queue | @NewAssignee becomes the task owner |
| 5 | Reassignment recorded with who and why | @ReassignedBy and @Reason stored |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-003 | Task Reassignment |
| UT-WFL-003 | btnReassign_Click |
| SP-WFL-004 | usp_Task_Reassign |

---

### User Story ID: US-WFL-004
**Title**: Open Related Entity from Task
**As a** claims adjuster
**I want to** quickly navigate to the policy or claim referenced by a task
**So that** I can access the entity without searching manually

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I must select a task before opening | No action if no row selected |
| 2 | Open navigates to the linked entity | Entity form opened based on EntityType and EntityID |
| 3 | Supports Policy, Claim, and Billing entities | Navigation works for all entity types [ASSUMPTION] |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-004 | Open Task Entity |
| UT-WFL-004 | btnOpen_Click |

---

### User Story ID: US-WFL-005
**Title**: View Notifications
**As a** system user
**I want to** see all notifications and alerts in a centralized view
**So that** I stay informed about tasks, claims updates, and system events

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Notification Center shows unread count prominently | lblUnreadCount in bold font |
| 2 | All notifications displayed in a grid | dgvNotifications shows list |
| 3 | I can refresh to check for new notifications | Refresh button updates display |
| 4 | Unread vs read visually distinguished | Unread count separate from total [ASSUMPTION] |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-005 | Notification Center View |
| UI-WFL-002 | frmNotificationCenter controls |
| SP-WFL-006 | usp_Notification_GetByUser |

---

### User Story ID: US-WFL-006
**Title**: Mark Notifications as Read
**As a** system user
**I want to** mark individual or all notifications as read
**So that** I can track which items I have already reviewed

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can mark a single notification as read | Select row, click Mark Read |
| 2 | Mark Read requires a selection | No action without selection |
| 3 | I can mark all notifications as read at once | Mark All Read button, no selection needed |
| 4 | Unread count updates after marking | lblUnreadCount refreshed [ASSUMPTION] |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-006 | Mark Notification as Read |
| FT-WFL-007 | Mark All Notifications as Read |
| UT-WFL-005 | btnMarkRead_Click |
| UT-WFL-006 | btnMarkAllRead_Click |
| SP-WFL-007 | usp_Notification_MarkRead |
| SP-WFL-008 | usp_Notification_MarkAllRead |

---

### User Story ID: US-WFL-007
**Title**: Delete Notifications
**As a** system user
**I want to** delete notifications I no longer need
**So that** my notification center stays clean and relevant

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I must select a notification before deleting | No action without selection |
| 2 | Deletion confirmation shown | "Notification deleted." message |
| 3 | Notification removed from list | Grid refreshed after deletion [ASSUMPTION] |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-008 | Delete Notification |
| UT-WFL-007 | btnDelete_Click |

---

### User Story ID: US-WFL-008
**Title**: Manage Diary Entries
**As a** claims adjuster
**I want to** search, complete, and reschedule diary/follow-up entries
**So that** I can track required follow-ups on my claims

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can search diary entries by date range | dtpDateFrom and dtpDateTo filters |
| 2 | Default search is last 7 days | dtpDateFrom = Today - 7 |
| 3 | I can filter by status | (All), PENDING, OVERDUE, COMPLETED |
| 4 | Search shows entry count | lblCount displays "N diary entries" |
| 5 | I can complete a diary entry | Select row, click Complete |
| 6 | I can reschedule a diary entry | Select row, click Reschedule |
| 7 | Both actions require a selection | No action without row selected |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-WFL-009 | Diary Manager Search and Filter |
| FT-WFL-010 | Diary Entry Completion and Rescheduling |
| UI-WFL-003 | frmDiaryManager controls |
| UT-WFL-009 | btnSearch_Click |
| UT-WFL-010 | btnComplete_Click |
| UT-WFL-011 | btnReschedule_Click |

---

### User Story ID: US-WFL-009
**Title**: Create Tasks for Team Members
**As a** team manager
**I want to** create tasks assigned to specific team members with priority and due dates
**So that** work is distributed and tracked across the team

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Task requires a type, subject, and assignee | @TaskType, @Subject, @AssignedTo required |
| 2 | Task can be linked to an entity | @EntityType and @EntityID optional |
| 3 | Priority defaults to NORMAL | @Priority defaults to 'NORMAL' |
| 4 | Due date is optional | @DueDate can be NULL |
| 5 | Description provides additional context | @Description VARCHAR(MAX) optional |
| 6 | Task ID returned on creation | @TaskID OUTPUT populated |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-WFL-001 | usp_Task_Create |
| BR-WFL-001 | Task Priority Rules |

---

### User Story ID: US-WFL-010
**Title**: Receive Notifications for Important Events
**As a** system user
**I want to** receive notifications when tasks are assigned, claims are updated, or system events occur
**So that** I can respond promptly to items requiring my attention

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Notifications created for task assignments | NotificationType='TASK_ASSIGNED' |
| 2 | Notifications can reference an entity | @EntityType and @EntityID link to source |
| 3 | Notifications include subject and message | Descriptive content for context |
| 4 | Notifications start as unread | IsRead = 0 initially [ASSUMPTION] |
| 5 | UserID targets specific recipient | Only that user sees the notification |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-WFL-005 | usp_Notification_Create |
| BR-WFL-002 | Notification Trigger Rules |
