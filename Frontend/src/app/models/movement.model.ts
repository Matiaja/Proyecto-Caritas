export interface Movement {
    donationRequestId: number;
    fromCenter: string;
    toCenter: string;
    productName: string;
    quantity: number;
    status: string;
    assignmentDate: string;
    updatedDate: string;
    description?: string;
}
