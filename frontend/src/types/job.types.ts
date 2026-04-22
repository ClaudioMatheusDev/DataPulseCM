export interface JobExecution {
  executionID: number;
  jobName: string;
  status: string;
  startDate: string;
  endDate?: string;
  executionDurationMs?: number;
  errorMessage?: string;
  rowsProcessed?: number;
  rowsInserted?: number;
  rowsUpdated?: number;
  rowsDeleted?: number;
  serverName?: string;
  databaseName?: string;
  source?: string;
  destination?: string;
  dataQualityScore?: number;
  correlationId?: string;
  tags?: string;
}

export interface Statistics {
  total: number;
  successful: number;
  failed: number;
  successRate: number;
  byStatus?: Record<string, number>;
  period?: {
    startDate?: string;
    endDate?: string;
  };
}

export interface JobExecutionDetail {
  detailId: number;
  executionId: number;
  stepName: string;
  stepOrder: number;
  stepStatus: string;
  stepMessage?: string;
  startDateTime: string;
  endDateTime?: string;
  durationInSeconds?: number;
}