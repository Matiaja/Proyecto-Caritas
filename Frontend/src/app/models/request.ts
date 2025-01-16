export interface Request {
    id: number;
    requestingCenterId: number;
    urgencyLevel: string;
    requestDate: Date | string;
    requestingCenter: {
        id: number;
        name: string;
    };
}
