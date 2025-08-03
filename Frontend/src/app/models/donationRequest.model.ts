import { CenterModel } from "./center.model";
import { OrderLine } from "./orderLine.model";

export interface DonationRequest {
  id?: number;
  orderLineId: number;
  orderLine?: OrderLine;
  assignedCenterId: number;
  assignedCenter?: CenterModel;
  quantity: number;
  assignmentDate?: Date;
  status?: string;
  lastStatusChangeDate?: Date;
  statusHistory?: {
    status: string;
    changeDate: Date;
  }[];
}
