import React, { useState, useEffect } from 'react';
import { X, User, Mail, Building, Briefcase, DollarSign, Calendar, Lock, Shield } from 'lucide-react';

export interface Employee {
  id?: number;
  firstName: string;
  lastName: string;
  email: string;
  password?: string;
  department: string;
  departmentId: number;
  roleId: number;
  position: string;
  salary: number;
  hireDate: string;
}

interface Department {
  id: number;
  name: string;
}

interface EmployeeModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (employee: Omit<Employee, 'id'>) => Promise<void>;
  initialData?: Employee | null;
}

const DEFAULT_DEPARTMENTS: Department[] = [
  { id: 1, name: 'Management' },
  { id: 2, name: 'Engineering' },
  { id: 3, name: 'Human Resources' },
  { id: 4, name: 'Sales & Marketing' }
];

export function EmployeeModal({ isOpen, onClose, onSave, initialData }: EmployeeModalProps) {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: 'pakistan12345',
    department: 'Engineering',
    departmentId: 2,
    roleId: 1,
    position: 'Software Engineer',
    salary: 75000,
    hireDate: new Date().toISOString().split('T')[0]
  });

  const [departments, setDepartments] = useState<Department[]>(DEFAULT_DEPARTMENTS);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    const fetchDepartments = async () => {
      try {
        const response = await fetch('http://localhost:5094/api/departments');
        if (response.ok) {
          const data = await response.json();
          if (Array.isArray(data) && data.length > 0) {
            setDepartments(data);
          }
        }
      } catch (err) {
        console.warn('Using fallback departments due to API error:', err);
      }
    };

    if (isOpen) {
      fetchDepartments();
    }
  }, [isOpen]);

  useEffect(() => {
    if (initialData) {
      setFormData({
        firstName: initialData.firstName || '',
        lastName: initialData.lastName || '',
        email: initialData.email || '',
        password: initialData.password || 'pakistan12345',
        department: initialData.department || 'Engineering',
        departmentId: initialData.departmentId || 2,
        roleId: initialData.roleId || 1,
        position: initialData.position || 'Software Engineer',
        salary: initialData.salary || 75000,
        hireDate: initialData.hireDate ? initialData.hireDate.split('T')[0] : new Date().toISOString().split('T')[0]
      });
    } else {
      setFormData({
        firstName: '',
        lastName: '',
        email: '',
        password: 'pakistan12345',
        department: 'Engineering',
        departmentId: 2,
        roleId: 1,
        position: 'Software Engineer',
        salary: 75000,
        hireDate: new Date().toISOString().split('T')[0]
      });
    }
    setValidationError(null);
  }, [initialData, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError(null);

    if (!formData.firstName.trim() || !formData.lastName.trim()) {
      setValidationError('First name and Last name are required.');
      return;
    }

    if (!formData.email.includes('@')) {
      setValidationError('Please enter a valid email address.');
      return;
    }

    if (!formData.password || formData.password.length < 6) {
      setValidationError('Password must be at least 6 characters.');
      return;
    }

    if (Number(formData.salary) <= 0) {
      setValidationError('Salary must be greater than zero.');
      return;
    }

    setIsSubmitting(true);
    try {
      const cleanPayload = {
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        email: formData.email.trim(),
        password: formData.password,
        department: formData.department,
        departmentId: Number(formData.departmentId),
        roleId: Number(formData.roleId),
        position: formData.position.trim(),
        salary: Number(formData.salary),
        hireDate: formData.hireDate ? new Date(formData.hireDate).toISOString() : new Date().toISOString()
      };

      await onSave(cleanPayload);
      onClose();
    } catch (error: any) {
      console.error('Submission error:', error);
      setValidationError(error.message || 'Failed to save employee record.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm animate-fadeIn">
      <div className="bg-[#101827] border border-slate-800/80 rounded-2xl w-full max-w-xl overflow-hidden shadow-2xl">
        <div className="flex justify-between items-center p-6 border-b border-slate-800/80 bg-[#0d1422]">
          <h2 className="text-xl font-bold text-white">
            {initialData?.id ? 'Edit Personnel Record' : 'Add New Employee'}
          </h2>
          <button 
            type="button"
            onClick={onClose}
            className="p-2 text-slate-400 hover:text-white hover:bg-slate-800/50 rounded-xl transition-colors"
          >
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {validationError && (
            <div className="p-3 bg-red-950/60 border border-red-800 rounded-xl text-red-300 text-xs">
              {validationError}
            </div>
          )}

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">First Name</label>
              <div className="relative">
                <User className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <input 
                  type="text" 
                  required
                  value={formData.firstName}
                  onChange={(e) => setFormData({...formData, firstName: e.target.value})}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
                  placeholder="John"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Last Name</label>
              <div className="relative">
                <User className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <input 
                  type="text" 
                  required
                  value={formData.lastName}
                  onChange={(e) => setFormData({...formData, lastName: e.target.value})}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
                  placeholder="Doe"
                />
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Email Address</label>
              <div className="relative">
                <Mail className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <input 
                  type="email" 
                  required
                  value={formData.email}
                  onChange={(e) => setFormData({...formData, email: e.target.value})}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
                  placeholder="john.doe@company.com"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Password</label>
              <div className="relative">
                <Lock className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <input 
                  type="password" 
                  required
                  value={formData.password}
                  onChange={(e) => setFormData({...formData, password: e.target.value})}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
                  placeholder="••••••••"
                />
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Department</label>
              <div className="relative">
                <Building className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <select 
                  value={formData.departmentId}
                  onChange={(e) => {
                    const selectedId = Number(e.target.value);
                    const foundDept = departments.find(d => d.id === selectedId);
                    setFormData({
                      ...formData, 
                      departmentId: selectedId,
                      department: foundDept ? foundDept.name : formData.department
                    });
                  }}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors appearance-none"
                >
                  {departments.map((dept) => (
                    <option key={dept.id} value={dept.id}>
                      {dept.name} (ID: {dept.id})
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Role ID</label>
              <div className="relative">
                <Shield className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <select 
                  value={formData.roleId}
                  onChange={(e) => setFormData({...formData, roleId: Number(e.target.value)})}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors appearance-none"
                >
                  <option value={1}>Role ID: 1 (Admin)</option>
                  <option value={2}>Role ID: 2 (Employee)</option>
                </select>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Position</label>
              <div className="relative">
                <Briefcase className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <input 
                  type="text" 
                  required
                  value={formData.position}
                  onChange={(e) => setFormData({...formData, position: e.target.value})}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
                  placeholder="Software Engineer"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Salary ($)</label>
              <div className="relative">
                <DollarSign className="absolute left-3.5 top-3 text-slate-500" size={16} />
                <input 
                  type="number" 
                  required
                  value={formData.salary}
                  onChange={(e) => setFormData({...formData, salary: Number(e.target.value)})}
                  className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
                  placeholder="75000"
                />
              </div>
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold uppercase tracking-wider text-slate-400 mb-1.5">Hire Date</label>
            <div className="relative">
              <Calendar className="absolute left-3.5 top-3 text-slate-500" size={16} />
              <input 
                type="date" 
                required
                value={formData.hireDate}
                onChange={(e) => setFormData({...formData, hireDate: e.target.value})}
                className="w-full bg-[#070b14] border border-slate-800 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-purple-500 transition-colors"
              />
            </div>
          </div>

          <div className="flex justify-end space-x-3 pt-4 border-t border-slate-800/80">
            <button 
              type="button"
              onClick={onClose}
              className="px-5 py-2.5 rounded-xl text-sm font-semibold text-slate-400 hover:text-white hover:bg-slate-800/50 transition-colors"
            >
              Cancel
            </button>
            <button 
              type="submit"
              disabled={isSubmitting}
              className="px-6 py-2.5 rounded-xl text-sm font-semibold bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white shadow-lg shadow-purple-600/25 transition-all disabled:opacity-50"
            >
              {isSubmitting ? 'Saving...' : initialData?.id ? 'Update Record' : 'Save Record'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}