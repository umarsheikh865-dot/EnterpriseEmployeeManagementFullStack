import React, { useEffect, useState } from 'react';
import apiClient from '../../services/apiClient';
import { UserPlus, Trash2, Edit } from 'lucide-react';

export const EmployeeList: React.FC = () => {
    const [employees, setEmployees] = useState<any[]>([]);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        fetchEmployees();
    }, []);

    const fetchEmployees = async () => {
        try {
            const response = await apiClient.get('/employees');
            setEmployees(response.data);
        } catch (err) {
            console.error('Failed to fetch employees', err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ backgroundColor: '#fff', padding: '24px', borderRadius: '12px', border: '1px solid #e2e8f0', boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h2 style={{ fontSize: '20px', fontWeight: 600, color: '#1e293b' }}>Employee Directory</h2>
                <button style={{ backgroundColor: '#7c3aed', color: '#fff', border: 'none', padding: '10px 16px', borderRadius: '6px', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '8px', cursor: 'pointer' }}>
                    <UserPlus size={18} /> Add Employee
                </button>
            </div>

            {loading ? (
                <p style={{ color: '#64748b' }}>Loading directory records...</p>
            ) : (
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                    <thead>
                        <tr style={{ borderBottom: '2px solid #f1f5f9', color: '#64748b', fontSize: '13px' }}>
                            <th style={{ padding: '12px' }}>Name</th>
                            <th style={{ padding: '12px' }}>Email</th>
                            <th style={{ padding: '12px' }}>Department</th>
                            <th style={{ padding: '12px' }}>Position</th>
                            <th style={{ padding: '12px', textAlign: 'right' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {employees.map((emp, idx) => (
                            <tr key={idx} style={{ borderBottom: '1px solid #f1f5f9', fontSize: '14px', color: '#334155' }}>
                                <td style={{ padding: '14px', fontWeight: 500 }}>{emp.firstName} {emp.lastName}</td>
                                <td style={{ padding: '14px' }}>{emp.email}</td>
                                <td style={{ padding: '14px' }}>{emp.departmentName || 'Engineering'}</td>
                                <td style={{ padding: '14px' }}>{emp.position || 'Developer'}</td>
                                <td style={{ padding: '14px', textAlign: 'right' }}>
                                    <button style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#64748b', marginRight: '10px' }}><Edit size={16} /></button>
                                    <button style={{ background: 'none', border: 'none', cursor: 'pointer', color: '#ef4444' }}><Trash2 size={16} /></button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
};