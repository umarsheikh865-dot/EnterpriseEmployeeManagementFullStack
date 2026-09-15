import React from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { Users, Camera, MessageSquare, LogOut, LayoutDashboard } from 'lucide-react';

export const MainLayout: React.FC = () => {
    const navigate = useNavigate();

    const handleLogout = () => {
        localStorage.removeItem('token');
        navigate('/login');
    };

    return (
        <div style={{ display: 'flex', height: '100vh', backgroundColor: '#f8fafc', fontFamily: 'Inter, sans-serif' }}>
            {/* Sidebar */}
            <aside style={{ width: '260px', backgroundColor: '#1e293b', color: '#fff', display: 'flex', flexDirection: 'column' }}>
                <div style={{ padding: '24px', fontSize: '18px', fontWeight: 700, borderBottom: '1px solid #334155' }}>
                    Enterprise Portal
                </div>
                <nav style={{ flex: 1, padding: '16px', display: 'flex', flexDirection: 'column', gap: '8px' }}>
                    <Link to="/" style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '12px', color: '#cbd5e1', textDecoration: 'none', borderRadius: '6px' }}>
                        <LayoutDashboard size={20} /> Dashboard
                    </Link>
                    <Link to="/employees" style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '12px', color: '#cbd5e1', textDecoration: 'none', borderRadius: '6px' }}>
                        <Users size={20} /> Employees
                    </Link>
                    <Link to="/attendance" style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '12px', color: '#cbd5e1', textDecoration: 'none', borderRadius: '6px' }}>
                        <Camera size={20} /> Biometric Kiosk
                    </Link>
                </nav>
                <div style={{ padding: '16px', borderTop: '1px solid #334155' }}>
                    <button onClick={handleLogout} style={{ width: '100%', display: 'flex', alignItems: 'center', gap: '10px', padding: '10px', backgroundColor: 'transparent', border: 'none', color: '#ef4444', cursor: 'pointer', fontWeight: 600 }}>
                        <LogOut size={18} /> Logout
                    </button>
                </div>
            </aside>

            {/* Main Content Area */}
            <main style={{ flex: 1, display: 'flex', flexDirection: 'column', overflowY: 'auto' }}>
                <header style={{ height: '70px', backgroundColor: '#fff', borderBottom: '1px solid #e2e8f0', display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '0 32px' }}>
                    <h1 style={{ fontSize: '18px', fontWeight: 600, color: '#1e293b' }}>Active Personnel & System Management</h1>
                    <div style={{ fontSize: '14px', color: '#64748b', fontWeight: 500 }}>System Status: <span style={{ color: '#22c55e' }}>● Operational</span></div>
                </header>
                <div style={{ padding: '32px', flex: 1 }}>
                    <Outlet />
                </div>
            </main>
        </div>
    );
};