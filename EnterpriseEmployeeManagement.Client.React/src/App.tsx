import React, { useState, useEffect } from 'react';
import { 
  Users, 
  Building2, 
  Search, 
  Plus, 
  LogOut, 
  ShieldCheck, 
  DollarSign, 
  Calendar,
  Edit,
  Trash2,
  AlertCircle,
  RefreshCw,
  Briefcase,
  ScanLine,
  CreditCard,
  Download,
  Shield
} from 'lucide-react';
import { EmployeeModal } from './components/EmployeeModal';
import { AIChatbot } from './components/AIChatbot';
import { FaceAttendance } from './components/FaceAttendance';
import { StripeCheckout } from './StripeBilling/StripeCheckout';
import apiClient from './services/apiClient';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

interface Employee {
  id?: number;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  departmentId: number;
  position: string;
  salary: number;
  hireDate: string;
}

interface AuditLog {
  id: number;
  action: string;
  details: string;
  timestamp: string;
}

export default function App() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [filteredEmployees, setFilteredEmployees] = useState<Employee[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isFaceModalOpen, setIsFaceModalOpen] = useState(false);
  const [isStripeModalOpen, setIsStripeModalOpen] = useState(false);
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);
  const [activeTab, setActiveTab] = useState<'directory' | 'departments' | 'attendance' | 'audit'>('directory');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [userRole, setUserRole] = useState<'Admin' | 'Employee'>('Admin');
   
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([
    { id: 1, action: 'SYSTEM_INIT', details: 'Enterprise Employee Management v2.0 loaded successfully.', timestamp: new Date().toLocaleTimeString() }
  ]);
   
  const [stats, setStats] = useState({
    totalEmployees: 0,
    activeDepartments: 2,
    avgSalary: 0
  });

  useEffect(() => {
    fetchEmployees();
  }, []);

  useEffect(() => {
    const query = searchQuery.toLowerCase().trim();
    if (!query) {
      setFilteredEmployees(employees);
    } else {
      const results = employees.filter(emp => 
        emp.firstName?.toLowerCase().includes(query) ||
        emp.lastName?.toLowerCase().includes(query) ||
        emp.email?.toLowerCase().includes(query) ||
        emp.department?.toLowerCase().includes(query) ||
        emp.position?.toLowerCase().includes(query)
      );
      setFilteredEmployees(results);
    }
  }, [searchQuery, employees]);

  const handleUnauthorized = () => {
    localStorage.removeItem('token');
    window.location.reload();
  };

  const addAuditTrail = (action: string, details: string) => {
    const newLog: AuditLog = {
      id: Date.now(),
      action,
      details,
      timestamp: new Date().toLocaleTimeString()
    };
    setAuditLogs(prev => [newLog, ...prev]);
  };

  const fetchEmployees = async () => {
    setIsLoading(true);
    try {
      const response = await apiClient.get('/employees');
      const data = response.data;
      const empList = data.items || data;
      setEmployees(empList);
      setFilteredEmployees(empList);

      const total = empList.length;
      const totalSal = empList.reduce((acc: number, curr: Employee) => acc + (curr.salary || 0), 0);
      setStats({
        totalEmployees: total,
        activeDepartments: 2,
        avgSalary: total > 0 ? Math.round(totalSal / total) : 0
      });
      setErrorMessage(null);
      addAuditTrail('FETCH_DATA', 'Successfully synchronized employee directory from backend.');
    } catch (error: any) {
      console.error('Error fetching employees:', error);
      if (error.response?.status === 401) {
        handleUnauthorized();
        return;
      }
      setErrorMessage('Network error: Unable to connect to backend API securely.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSaveEmployee = async (formData: Omit<Employee, 'id'>) => {
    if (userRole !== 'Admin') {
      toast.error('Permission Denied: Only Admin role can modify records.');
      return;
    }
    const isEditing = !!selectedEmployee?.id;
    try {
      if (isEditing) {
        await apiClient.put(`/employees/${selectedEmployee.id}`, formData);
        toast.success('Employee updated successfully!');
        addAuditTrail('UPDATE_EMPLOYEE', `Updated record for ${formData.firstName} ${formData.lastName}`);
      } else {
        await apiClient.post('/employees', formData);
        toast.success('Employee added successfully!');
        addAuditTrail('CREATE_EMPLOYEE', `Created record for ${formData.firstName} ${formData.lastName}`);
      }
      await fetchEmployees();
    } catch (err: any) {
      const msg = err.response?.data?.detail || 'Failed to save employee record';
      toast.error(msg);
      throw new Error(msg);
    }
  };

  const handleDelete = async (id?: number) => {
    if (userRole !== 'Admin') {
      toast.error('Permission Denied: Only Admin can delete records.');
      return;
    }
    if (!id || !confirm('Are you sure you want to delete this personnel record?')) return;
    try {
      await apiClient.delete(`/employees/${id}`);
      toast.success('Employee record deleted successfully!');
      addAuditTrail('DELETE_EMPLOYEE', `Deleted employee ID: ${id}`);
      fetchEmployees();
    } catch (error) {
      console.error('Delete error:', error);
      toast.error('Network error during deletion.');
    }
  };

  const exportToCSV = () => {
    const headers = "ID,First Name,Last Name,Email,Department,Position,Salary\n";
    const rows = employees.map(e => `${e.id},${e.firstName},${e.lastName},${e.email},${e.department || 'Dept ' + e.departmentId},${e.position},${e.salary}`).join("\n");
    const blob = new Blob([headers + rows], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Employee_Directory_${new Date().toISOString().slice(0,10)}.csv`;
    a.click();
    toast.success('Report exported to CSV successfully!');
    addAuditTrail('EXPORT_DATA', 'Exported employee directory to CSV.');
  };

  return (
    <div className="portal-container">
      <ToastContainer position="top-right" autoClose={3000} theme="dark" />

      {errorMessage && (
        <div style={{ backgroundColor: '#450a0a', borderBottom: '1px solid #7f1d1d', padding: '10px 24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: '12px', color: '#fca5a5' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <AlertCircle size={16} />
            <span>{errorMessage}</span>
          </div>
          <button onClick={() => setErrorMessage(null)} style={{ background: 'none', border: 'none', color: 'white', cursor: 'pointer', textDecoration: 'underline' }}>Dismiss</button>
        </div>
      )}

      {/* Top Bar */}
      <header className="top-bar">
        <div className="security-badge">
          <ShieldCheck size={16} />
          <span>ENTERPRISE SECURITY VERIFIED</span>
          <span style={{ margin: '0 8px', color: '#4b5563' }}>|</span>
          <span style={{ color: '#10b981', display: 'flex', alignItems: 'center', gap: '4px' }}>
            <span style={{ width: '8px', height: '8px', borderRadius: '50%', backgroundColor: '#10b981', display: 'inline-block' }}></span>
            JWT Session Active
          </span>
        </div>
         
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flexWrap: 'wrap' }}>
          <button 
            onClick={() => {
              const nextRole = userRole === 'Admin' ? 'Employee' : 'Admin';
              setUserRole(nextRole);
              toast.info(`Switched active role to: ${nextRole}`);
            }}
            style={{ background: 'rgba(234, 179, 8, 0.1)', border: '1px solid rgba(234, 179, 8, 0.3)', color: '#facc15', padding: '6px 12px', borderRadius: '8px', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '6px', fontSize: '12px', fontWeight: 600 }}
            title="Toggle Role for RBAC Demo"
          >
            <Shield size={14} /> Role: {userRole}
          </button>

          <button 
            onClick={() => setIsFaceModalOpen(true)}
            style={{ background: 'rgba(139, 92, 246, 0.1)', border: '1px solid rgba(139, 92, 246, 0.3)', color: '#a78bfa', padding: '6px 12px', borderRadius: '8px', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '6px', fontSize: '12px', fontWeight: 600 }}
          >
            <ScanLine size={14} /> Face ID Scan
          </button>

          {userRole === 'Admin' && (
            <button 
              onClick={() => setIsStripeModalOpen(true)}
              style={{ background: 'rgba(59, 130, 246, 0.1)', border: '1px solid rgba(59, 130, 246, 0.3)', color: '#60a5fa', padding: '6px 12px', borderRadius: '8px', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '6px', fontSize: '12px', fontWeight: 600 }}
            >
              <CreditCard size={14} /> Stripe Payroll
            </button>
          )}

          <button 
            onClick={fetchEmployees} 
            style={{ background: 'none', border: 'none', color: '#9ca3af', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '4px', marginLeft: '8px' }}
          >
            <RefreshCw size={14} className={isLoading ? "animate-spin" : ""} />
            <span>Sync</span>
          </button>

          <button onClick={handleUnauthorized} className="disconnect-btn">
            <LogOut size={14} />
            <span>Disconnect</span>
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="main-content">
        <div className="header-row">
          <div>
            <h1 className="portal-title">Employee Management Portal</h1>
            <p className="portal-subtitle">High-performance ASP.NET Core backend coupled with a responsive Vite React UI.</p>
          </div>
           
          <div style={{ display: 'flex', gap: '12px' }}>
            <button 
              onClick={exportToCSV}
              style={{ background: 'rgba(16, 185, 129, 0.1)', border: '1px solid rgba(16, 185, 129, 0.3)', color: '#34d399', padding: '10px 16px', borderRadius: '8px', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '8px', fontWeight: 600, fontSize: '13px' }}
            >
              <Download size={16} /> Export CSV
            </button>

            {userRole === 'Admin' && (
              <button 
                onClick={() => { setSelectedEmployee(null); setIsModalOpen(true); }}
                className="btn-primary"
              >
                <Plus size={18} />
                <span>Add New Employee</span>
              </button>
            )}
          </div>
        </div>

        {/* Stats Grid */}
        <div className="stats-grid">
          <div className="stat-card">
            <div>
              <p className="stat-title">Total Personnel</p>
              <h3 className="stat-value">{stats.totalEmployees}</h3>
            </div>
            <div className="stat-icon">
              <Users size={24} />
            </div>
          </div>

          <div className="stat-card">
            <div>
              <p className="stat-title">Active Departments</p>
              <h3 className="stat-value">{stats.activeDepartments}</h3>
            </div>
            <div className="stat-icon" style={{ background: 'rgba(59, 130, 246, 0.1)', color: '#60a5fa' }}>
              <Building2 size={24} />
            </div>
          </div>

          <div className="stat-card">
            <div>
              <p className="stat-title">Average Compensation</p>
              <h3 className="stat-value">${stats.avgSalary.toLocaleString()}</h3>
            </div>
            <div className="stat-icon" style={{ background: 'rgba(16, 185, 129, 0.1)', color: '#34d399' }}>
              <DollarSign size={24} />
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="tabs-nav">
          <button 
            onClick={() => setActiveTab('directory')}
            className={`tab-btn ${activeTab === 'directory' ? 'active' : ''}`}
            style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
          >
            <Users size={16} />
            <span>Directory Overview</span>
          </button>
          <button 
            onClick={() => setActiveTab('departments')}
            className={`tab-btn ${activeTab === 'departments' ? 'active' : ''}`}
            style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
          >
            <Building2 size={16} />
            <span>Department Registry</span>
          </button>
          <button 
            onClick={() => setActiveTab('attendance')}
            className={`tab-btn ${activeTab === 'attendance' ? 'active' : ''}`}
            style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
          >
            <Calendar size={16} />
            <span>Attendance Logs</span>
          </button>
          <button 
            onClick={() => setActiveTab('audit')}
            className={`tab-btn ${activeTab === 'audit' ? 'active' : ''}`}
            style={{ display: 'flex', alignItems: 'center', gap: '8px' }}
          >
            <Shield size={16} />
            <span>Security Audit Trail</span>
          </button>
        </div>

        {/* Tab 1: Directory Overview */}
        {activeTab === 'directory' && (
          <div className="card-box">
            <div className="card-header">
              <div>
                <h3 style={{ margin: 0, fontSize: '16px', fontWeight: '700', color: 'white' }}>Personnel Directory</h3>
                <p style={{ margin: '4px 0 0 0', fontSize: '12px', color: '#9ca3af' }}>Manage active personnel records securely with real-time API sync</p>
              </div>
              <div style={{ position: 'relative' }}>
                <Search style={{ position: 'absolute', left: '12px', top: '10px', color: '#6b7280' }} size={16} />
                <input 
                  type="text" 
                  placeholder="Search records..." 
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="search-input"
                />
              </div>
            </div>

            <div style={{ overflowX: 'auto' }}>
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Full Name</th>
                    <th>Email Address</th>
                    <th>Department</th>
                    <th>Position</th>
                    <th>Salary</th>
                    <th style={{ textAlign: 'right' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {isLoading ? (
                    <tr>
                      <td colSpan={6} style={{ textAlign: 'center', padding: '40px', color: '#9ca3af' }}>
                        <RefreshCw size={24} className="animate-spin" style={{ margin: '0 auto 8px auto', display: 'block', color: '#8b5cf6' }} />
                        Loading records from backend...
                      </td>
                    </tr>
                  ) : filteredEmployees.length > 0 ? (
                    filteredEmployees.map((emp) => (
                      <tr key={emp.id || emp.email}>
                        <td style={{ fontWeight: '600', color: 'white' }}>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
                            <div style={{ width: '32px', height: '32px', borderRadius: '8px', background: 'rgba(139, 92, 246, 0.1)', color: '#a78bfa', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '12px', fontWeight: 'bold' }}>
                              {emp.firstName?.[0]}{emp.lastName?.[0]}
                            </div>
                            <span>{emp.firstName} {emp.lastName}</span>
                          </div>
                        </td>
                        <td style={{ color: '#9ca3af' }}>{emp.email}</td>
                        <td>
                          <span style={{ padding: '4px 10px', background: 'rgba(139, 92, 246, 0.1)', color: '#a78bfa', borderRadius: '6px', fontSize: '12px', fontWeight: '500' }}>
                            {emp.department || `Dept ID: ${emp.departmentId}`}
                          </span>
                        </td>
                        <td style={{ color: '#d1d5db', display: 'flex', alignItems: 'center', gap: '6px', paddingTop: '20px' }}>
                          <Briefcase size={14} color="#6b7280" />
                          <span>{emp.position}</span>
                        </td>
                        <td style={{ fontWeight: 'bold', color: '#34d399' }}>${emp.salary?.toLocaleString()}</td>
                        <td style={{ textAlign: 'right' }}>
                          {userRole === 'Admin' ? (
                            <>
                              <button 
                                onClick={() => { setSelectedEmployee(emp); setIsModalOpen(true); }}
                                className="action-icon-btn"
                                title="Edit Record"
                              >
                                <Edit size={16} />
                              </button>
                              <button 
                                onClick={() => handleDelete(emp.id)}
                                className="action-icon-btn delete"
                                title="Delete Record"
                              >
                                <Trash2 size={16} />
                              </button>
                          </>
                        ) : (
                          <span style={{ fontSize: '12px', color: '#6b7280', fontStyle: 'italic' }}>View Only</span>
                        )}
                      </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={6} style={{ textAlign: 'center', padding: '40px', color: '#9ca3af' }}>
                        No employee records found matching your query.
                      </td>
                    </tr>
                  )}
              </tbody>
            </table>
          </div>
        </div>
        )}

        {/* Tab 2: Department Registry */}
        {activeTab === 'departments' && (
          <div className="card-box" style={{ padding: '40px', textAlign: 'center' }}>
            <Building2 size={48} style={{ margin: '0 auto 16px auto', color: '#60a5fa', background: 'rgba(59, 130, 246, 0.1)', padding: '12px', borderRadius: '12px' }} />
            <h3 style={{ color: 'white', fontSize: '20px', margin: '0 0 8px 0' }}>Department Management Registry</h3>
            <p style={{ color: '#9ca3af', fontSize: '14px', margin: 0 }}>Backend relational mapping is fully active for Engineering and Operations departments.</p>
          </div>
        )}

        {/* Tab 3: Attendance Logs */}
        {activeTab === 'attendance' && (
          <div className="card-box" style={{ padding: '40px', textAlign: 'center' }}>
            <Calendar size={48} style={{ margin: '0 auto 16px auto', color: '#34d399', background: 'rgba(16, 185, 129, 0.1)', padding: '12px', borderRadius: '12px' }} />
            <h3 style={{ color: 'white', fontSize: '20px', margin: '0 0 8px 0' }}>Attendance & Biometric Check-In Logs</h3>
            <p style={{ color: '#9ca3af', fontSize: '14px', margin: 0 }}>Daily personnel punch-in logs connected directly to backend facial recognition system.</p>
          </div>
        )}

        {/* Tab 4: Audit Trail */}
        {activeTab === 'audit' && (
          <div className="card-box">
            <div className="card-header">
              <div>
                <h3 style={{ margin: 0, fontSize: '16px', fontWeight: '700', color: 'white' }}>Security Audit Trail</h3>
                <p style={{ margin: '4px 0 0 0', fontSize: '12px', color: '#9ca3af' }}>Real-time log of administrative actions and API requests</p>
              </div>
            </div>
            <div style={{ overflowX: 'auto', padding: '16px' }}>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>Action Type</th>
                  <th>Details</th>
                </tr>
              </thead>
              <tbody>
                {auditLogs.map((log) => (
                  <tr key={log.id}>
                    <td style={{ color: '#9ca3af', fontFamily: 'monospace' }}>{log.timestamp}</td>
                  <td>
                    <span style={{ padding: '4px 8px', background: 'rgba(59, 130, 246, 0.1)', color: '#60a5fa', borderRadius: '4px', fontSize: '11px', fontWeight: 'bold' }}>
                      {log.action}
                    </span>
                  </td>
                  <td style={{ color: '#e2e8f0' }}>{log.details}</td>
                </tr>
              ))}
              </tbody>
          </table>
          </div>
        </div>
        )}
      </main>

      {/* Modals & AI Assistant */}
      <EmployeeModal 
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSave={handleSaveEmployee}
        initialData={selectedEmployee}
      />
      <FaceAttendance isOpen={isFaceModalOpen} onClose={() => setIsFaceModalOpen(false)} />
      <StripeCheckout isOpen={isStripeModalOpen} onClose={() => setIsStripeModalOpen(false)} />
      <AIChatbot />
  </div>
  );
}