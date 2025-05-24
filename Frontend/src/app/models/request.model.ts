import { OrderLine } from './orderLine.model';

export interface RequestModel {
  id?: number;
  requestingCenterId: number;
  urgencyLevel: string;
  requestDate: Date | string;
  status: string;
  requestingCenter?: {
    id: number;
    name: string;
  };
  orderLines: OrderLine[];
}
