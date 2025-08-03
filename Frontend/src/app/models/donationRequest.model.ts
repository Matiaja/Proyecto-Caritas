import { CenterModel } from "./center.model";
import { OrderLine } from "./orderLine.model";

export interface DonationRequest {
  id?: number;
  orderLineId: number;
  orderLine?: OrderLine;
  assignedCenterId: number;
  assignedCenter?: CenterModel;
  quantity: number;
  assignmentDate?: Date | string;
  status?: string;
  lastStatusChangeDate?: Date | string;
  statusHistory?: {
    status: string;
    changeDate: Date | string;
  }[];
}
