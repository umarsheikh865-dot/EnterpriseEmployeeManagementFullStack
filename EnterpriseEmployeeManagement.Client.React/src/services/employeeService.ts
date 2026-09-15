import apiClient from './apiClient';

export interface Employee {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    department: string;
    position: string;
    salary: number;
    hireDate: string;
}

export const employeeService = {
    getAll: async () => {
        const response = await apiClient.get<Employee[]>('/employees');
        return response.data;
    },
    getById: async (id: number) => {
        const response = await apiClient.get<Employee>(`/employees/${id}`);
        return response.data;
    },
    create: async (employee: Omit<Employee, 'id'>) => {
        const response = await apiClient.post<Employee>('/employees', employee);
        return response.data;
    },
    update: async (id: number, employee: Employee) => {
        const response = await apiClient.put(`/employees/${id}`, employee);
        return response.data;
    },
    delete: async (id: number) => {
        await apiClient.delete(`/employees/${id}`);
    },
};
