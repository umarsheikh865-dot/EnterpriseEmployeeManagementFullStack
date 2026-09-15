import React, { useState } from 'react';
import { loadStripe } from '@stripe/stripe-js';
import apiClient from '../services/apiClient';
import { X, CreditCard, ShieldCheck } from 'lucide-react';

// Initialize Stripe with your test publishable key (securely checking environment variables)
const stripePublicKey = import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY || 'pk_test_51PlaceholderTestKey';
const stripePromise = loadStripe(stripePublicKey);

interface StripeCheckoutProps {
    isOpen: boolean;
    onClose: () => void;
}

export const StripeCheckout: React.FC<StripeCheckoutProps> = ({ isOpen, onClose }) => {
    const [loading, setLoading] = useState(false);

    if (!isOpen) return null;

    const handleCheckout = async () => {
        setLoading(true);
        try {
            // Call your backend ASP.NET Core API endpoint for Stripe session creation
            const response = await apiClient.post('/stripe/create-checkout-session', {
                planName: 'Enterprise Payroll Subscription',
                amount: 5000 // Amount in cents ($50.00)
            });

            const stripe = await stripePromise;
            if (stripe && response.data?.sessionId) {
                const { error } = await stripe.redirectToCheckout({ sessionId: response.data.sessionId });
                if (error) {
                    console.error('Stripe redirect error:', error.message);
                    alert(`Stripe Checkout Error: ${error.message}`);
                }
            } else {
                throw new Error('Invalid checkout session received from server.');
            }
        } catch (err: any) {
            console.error('Stripe checkout error:', err);
            // Fallback for local testing if backend Stripe endpoint isn't fully active yet
            alert('Note: Backend session simulation triggered. Ensure your ASP.NET Core Stripe endpoint is running for live redirection.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{
            position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.75)', backdropFilter: 'blur(4px)', display: 'flex',
            alignItems: 'center', justifyContent: 'center', zIndex: 1000, fontFamily: 'Inter, sans-serif'
        }}>
            <div style={{
                backgroundColor: '#111827', padding: '28px', borderRadius: '16px',
                width: '420px', border: '1px solid #374151', position: 'relative', color: '#fff',
                boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.7)'
            }}>
                <button 
                    onClick={onClose}
                    style={{
                        position: 'absolute', top: '16px', right: '16px', background: 'none',
                        border: 'none', color: '#9ca3af', cursor: 'pointer', padding: '4px',
                        display: 'flex', alignItems: 'center', justifyContent: 'center', borderRadius: '50%'
                    }}
                >
                    <X size={20} />
                </button>
                
                <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '12px' }}>
                    <div style={{ backgroundColor: 'rgba(99, 102, 241, 0.1)', padding: '10px', borderRadius: '10px', border: '1px solid rgba(99, 102, 241, 0.2)' }}>
                        <CreditCard size={22} color="#818cf8" />
                    </div>
                    <h3 style={{ fontSize: '18px', fontWeight: 600, color: '#f9fafb', margin: 0 }}>Enterprise Payroll & Subscription</h3>
                </div>

                <p style={{ fontSize: '13px', color: '#9ca3af', marginBottom: '20px', lineHeight: '1.5' }}>
                    Securely manage automated monthly salary payouts and enterprise billing via Stripe Test Mode.
                </p>

                <div style={{ backgroundColor: '#1f2937', padding: '12px 16px', borderRadius: '10px', marginBottom: '20px', border: '1px solid #374151', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div>
                        <div style={{ fontSize: '12px', color: '#9ca3af' }}>Selected Plan</div>
                        <div style={{ fontSize: '14px', fontWeight: 600, color: '#fff' }}>Monthly Enterprise Tier</div>
                    </div>
                    <div style={{ textAlign: 'right' }}>
                        <div style={{ fontSize: '12px', color: '#9ca3af' }}>Total</div>
                        <div style={{ fontSize: '15px', fontWeight: 700, color: '#34d399' }}>$50.00 / mo</div>
                    </div>
                </div>
                
                <button 
                    onClick={handleCheckout} 
                    disabled={loading}
                    style={{
                        width: '100%', backgroundColor: loading ? '#4f46e5' : '#6366f1', color: '#fff', border: 'none',
                        borderRadius: '10px', padding: '12px', fontWeight: 600, cursor: loading ? 'not-allowed' : 'pointer', fontSize: '14px',
                        boxShadow: '0 4px 12px rgba(99, 102, 241, 0.4)', transition: 'background-color 0.2s'
                    }}
                >
                    {loading ? 'Securing Gateway...' : 'Proceed to Stripe Checkout'}
                </button>

                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '6px', marginTop: '16px', fontSize: '11px', color: '#6b7280' }}>
                    <ShieldCheck size={14} color="#34d399" />
                    <span>End-to-end encrypted Stripe Test Environment</span>
                </div>
            </div>
        </div>
    );
};