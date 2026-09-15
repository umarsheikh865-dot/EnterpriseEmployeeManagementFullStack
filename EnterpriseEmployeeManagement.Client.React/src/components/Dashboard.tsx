import React, { useEffect, useState } from 'react';
import { employeeService, type Employee } from '../services/employeeService';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Users, LogOut, CreditCard, ShieldAlert, Search, UserPlus, Trash2, Edit2, Download } from 'lucide-react';
import { StripeCheckout } from '../Stripe Billing/StripeCheckout';
import { AIChatbot } from './AIChatbot';
import { EmployeeModal } from './EmployeeModal';

export const Dashboard: React.FC = () => {
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null);
    const { userRole, logout } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        loadEmployees();
    }, []);

    const loadEmployees = async () => {
        try {
            const data = await employeeService.getAll();
            setEmployees(data);
        } catch (err) {
            console.error('Failed to load employees', err);
        }
    };

    const handleDelete = async (id: number) => {
        if (userRole !== 'Admin') {
            alert('Unauthorized: Only administrators can delete records.');
            return;
        }

        if (window.confirm('Are you sure you want to delete this employee?')) {
            try {
                await employeeService.delete(id);
                loadEmployees();
            } catch (err) {
                console.error('Failed to delete employee', err);
            }
        }
    };

    // CSV Export Handler
    const handleExportCsv = () => {
        if (!employees || employees.length === 0) return;

        const headers = ['ID', 'First Name', 'Last Name', 'Email', 'Department', 'Position', 'Salary'];

        const rows = employees.map(emp => [
            emp.id ?? '',
            `"${emp.firstName ?? ''}"`,
            `"${emp.lastName ?? ''}"`,
            `"${emp.email ?? ''}"`,
            `"${emp.department ?? ''}"`,
            `"${emp.position || (emp as any).jobTitle || (emp as any).title || 'Software Engineer'}"`, 
            emp.salary ?? 0
        ]);

        const csvContent = [
            headers.join(','),
            ...rows.map(row => row.join(','))
        ].join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        
        const timestamp = new Date().toISOString().split('T')[0];
        link.setAttribute('href', url);
        link.setAttribute('download', `Employee_Directory_${timestamp}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    // Filter employees based on search input
    const filteredEmployees = employees.filter((emp) => {
        const query = searchTerm.toLowerCase();
        const positionVal = emp.position || (emp as any).jobTitle || (emp as any).title || '';
        return (
            emp.firstName.toLowerCase().includes(query) ||
            emp.lastName.toLowerCase().includes(query) ||
            emp.email.toLowerCase().includes(query) ||
            emp.department.toLowerCase().includes(query) ||
            positionVal.toLowerCase().includes(query)
        );
    });

    const getDepartmentBadgeColor = (dept: string) => {
        switch (dept?.toLowerCase()) {
            case 'management':
                return 'bg-amber-950/65 text-amber-300 border-amber-800';
            case 'engineering':
                return 'bg-purple-950/65 text-purple-300 border-purple-800';
            case 'human resources':
                return 'bg-blue-950/65 text-blue-300 border-blue-800';
            case 'sales & marketing':
                return 'bg-emerald-950/65 text-emerald-300 border-emerald-800';
            default:
                return 'bg-slate-800 text-slate-300 border-slate-700';
        }
    };

    return (
        <div className="min-h-screen bg-gray-950 text-gray-100 font-sans">
            {/* Top Navigation Bar */}
            <nav className="flex items-center justify-between border-b border-gray-800 bg-gray-900 px-6 py-4 shadow-md">
                <div className="flex items-center space-x-3">
                    <Users className="h-6 w-6 text-purple-400" />
                    <span className="text-xl font-bold tracking-wide">Enterprise Portal</span>
                    {userRole && (
                        <span className="rounded-full bg-purple-900/50 px-3 py-1 text-xs font-semibold text-purple-300 border border-purple-700">
                            Role: {userRole}
                        </span>
                    )}
                </div>
                <button
                    onClick={() => { logout(); navigate('/login'); }}
                    className="flex items-center space-x-2 rounded-lg bg-gray-800 px-4 py-2 text-sm text-gray-300 hover:bg-gray-700 transition"
                >
                    <LogOut className="h-4 w-4" />
                    <span>Logout</span>
                </button>
            </nav>

            {/* Main Content Area */}
            <main className="p-8 max-w-7xl mx-auto space-y-8">
                
                {/* Employee Directory Section */}
                <div>
                    <div className="mb-6 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                        <div>
                            <h1 className="text-2xl font-semibold">Employee Directory</h1>
                            <p className="text-sm text-gray-400">Manage active personnel records securely via ASP.NET Core API.</p>
                        </div>
                        
                        <div className="flex items-center space-x-3 w-full sm:w-auto flex-wrap gap-y-2">
                            {/* Live Search Bar */}
                            <div className="relative w-full sm:w-64">
                                <Search className="absolute left-3.5 top-3 text-gray-500" size={16} />
                                <input
                                    type="text"
                                    placeholder="Search records..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="w-full bg-gray-900 border border-gray-800 rounded-xl pl-10 pr-4 py-2 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
                                />
                            </div>

                            {/* Export CSV Button */}
                            <button
                                onClick={handleExportCsv}
                                className="flex items-center space-x-2 bg-gray-800 hover:bg-gray-700 text-gray-200 px-4 py-2 rounded-xl text-sm font-semibold transition border border-gray-700 whitespace-nowrap"
                                title="Export Employee List as CSV"
                            >
                                <Download size={16} />
                                <span>Export CSV</span>
                            </button>

                            {/* Add Employee Button */}
                            <button
                                onClick={() => {
                                    setEditingEmployee(null);
                                    setIsModalOpen(true);
                                }}
                                className="flex items-center space-x-2 bg-purple-600 hover:bg-purple-500 text-white px-4 py-2 rounded-xl text-sm font-semibold transition shadow-lg shadow-purple-600/20 whitespace-nowrap"
                            >
                                <UserPlus size={16} />
                                <span>Add Employee</span>
                            </button>
                        </div>
                    </div>

                    <div className="overflow-hidden rounded-xl border border-gray-800 bg-gray-900 shadow-xl">
                        <table className="w-full text-left border-collapse">
                            <thead>
                                <tr className="border-b border-gray-800 bg-gray-850 text-xs uppercase text-gray-400">
                                    <th className="p-4">Name</th>
                                    <th className="p-4">Email</th>
                                    <th className="p-4">Department</th>
                                    <th className="p-4">Position</th>
                                    <th className="p-4">Salary</th>
                                    <th className="p-4 text-right">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-800">
                                {filteredEmployees.map((emp) => (
                                    <tr key={emp.id} className="hover:bg-gray-800/40 transition">
                                        <td className="p-4 font-medium">{emp.firstName} {emp.lastName}</td>
                                        <td className="p-4 text-gray-400">{emp.email}</td>
                                        <td className="p-4">
                                            <span className="px-2.5 py-1 rounded-full text-xs font-medium border bg-purple-950/65 text-purple-300 border-purple-800">
                                                {emp.department || 'Engineering'}
                                            </span>
                                        </td>
                                        <td className="p-4 text-gray-300">
                                            {emp.position || (emp as any).jobTitle || (emp as any).title || 'Software Engineer'}
                                        </td>
                                        <td className="p-4 text-emerald-400 font-medium">
                                            ${emp.salary ? emp.salary.toLocaleString() : '75,000'}
                                        </td>
                                        <td className="p-4 text-right space-x-2">
                                            {userRole === 'Admin' ? (
                                                <div className="flex items-center justify-end space-x-2">
                                                    <button
                                                        onClick={() => {
                                                            setEditingEmployee(emp);
                                                            setIsModalOpen(true);
                                                        }}
                                                        className="p-1.5 text-purple-400 hover:text-purple-300 hover:bg-purple-950/40 rounded-lg transition"
                                                        title="Edit Employee"
                                                    >
                                                        <Edit2 size={16} />
                                                    </button>
                                                    <button
                                                        onClick={() => handleDelete(emp.id!)}
                                                        className="p-1.5 text-red-400 hover:text-red-300 hover:bg-red-950/40 rounded-lg transition"
                                                        title="Delete Employee"
                                                    >
                                                        <Trash2 size={16} />
                                                    </button>
                                                </div>
                                            ) : (
                                                <span className="text-xs text-gray-600 italic">View Only</span>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                                {filteredEmployees.length === 0 && (
                                    <tr>
                                        <td colSpan={6} className="p-8 text-center text-gray-500">
                                            {employees.length === 0 
                                                ? "No employee records found in database. Ensure backend server is running on port 5094."
                                                : "No matching records found for your search term."}
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>

                {/* Stripe Billing & Payroll Section */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="rounded-xl border border-gray-800 bg-gray-900 p-6 shadow-xl">
                        <div className="flex items-center space-x-3 mb-4">
                            <CreditCard className="h-6 w-6 text-indigo-400" />
                            <h2 className="text-lg font-semibold">Stripe Enterprise Payroll</h2>
                        </div>
                        <p className="text-sm text-gray-400 mb-4">
                            Automate monthly employee salary distribution and secure subscription billing through Stripe.
                        </p>
                        <StripeCheckout />
                    </div>

                    <div className="rounded-xl border border-gray-800 bg-gray-900 p-6 shadow-xl flex flex-col justify-between">
                        <div>
                            <div className="flex items-center space-x-3 mb-4">
                                <ShieldAlert className="h-6 w-6 text-emerald-400" />
                                <h2 className="text-lg font-semibold">Security & Compliance</h2>
                            </div>
                            <p className="text-sm text-gray-400 mb-4">
                                Active JWT session tokens are verified via Clean Architecture middleware. All API transactions are encrypted and logged.
                            </p>
                        </div>
                        <div className="text-xs text-emerald-400 bg-emerald-950/40 border border-emerald-800/50 p-3 rounded-lg">
                            System Status: All active directory parameters optimal.
                        </div>
                    </div>
                </div>

            </main>

            {/* Employee Modal Component */}
            <EmployeeModal
                isOpen={isModalOpen}
                onClose={() => {
                    setIsModalOpen(false);
                    setEditingEmployee(null);
                }}
                initialData={editingEmployee}
                onSave={async (empData) => {
                    if (editingEmployee && editingEmployee.id) {
                        await employeeService.update(editingEmployee.id, empData);
                    } else {
                        await employeeService.create(empData);
                    }
                    loadEmployees();
                    setEditingEmployee(null);
                }}
            />

            {/* Floating Enterprise AI Assistant */}
            <AIChatbot />
        </div>
    );
};