export class dtpResponse {
  payload: dtp;
}

export class dtp {
  planDates: Array<planDate>;
  amountDue: number;
  spendPerWeek: number;
  startDate: Date;
  endDate: Date;
  weeksRemaining: number;
  remaining: number;
}

export class planDate {
  transactionName: string;
  amount: number;
  date: Date;
}
