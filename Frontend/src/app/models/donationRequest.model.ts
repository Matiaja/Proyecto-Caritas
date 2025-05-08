import { CenterModel } from "./center.model";

export interface DonationRequest {
  id?: number;
  orderLineId: number;
  assignedCenterId: number;
  assignedCenter?: CenterModel;
  quantity: number;
  status?: string;
}
