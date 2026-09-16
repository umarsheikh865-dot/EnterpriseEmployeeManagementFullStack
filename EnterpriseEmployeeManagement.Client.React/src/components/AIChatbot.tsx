import React, { useState } from 'react';
import { MessageSquare, X, Send, Bot, User, Sparkles } from 'lucide-react';
import { type Employee } from '../services/employeeService';

interface AIChatbotProps {
    employees?: Employee[];
}

interface Message {
    sender: 'user' | 'ai';
    text: string;
}

export const AIChatbot: React.FC<AIChatbotProps> = ({ employees = [] }) => {
    const [isOpen, setIsOpen] = useState(false);
    const [input, setInput] = useState('');
    const [messages, setMessages] = useState<Message[]>([
        { 
            sender: 'ai', 
            text: 'Hello! I am your Enterprise AI Assistant. Ask me about employee counts, departments, specific staff members, or average salaries.' 
        }
    ]);

    const handleSend = (e: React.FormEvent) => {
        e.preventDefault();
        if (!input.trim()) return;

        const userMessage = input.trim();
        setMessages(prev => [...prev, { sender: 'user', text: userMessage }]);
        setInput('');

        // Smart dynamic query parsing
        setTimeout(() => {
            const lowerQuery = userMessage.toLowerCase();
            let aiResponse = "I'm not quite sure how to answer that. Try asking about 'total employees', 'departments', 'average salary', or search for a specific name.";

            if (lowerQuery.includes('hello') || lowerQuery.includes('hi') || lowerQuery.includes('hey')) {
                aiResponse = "Hello! How can I assist you with your enterprise management tasks today?";
            } 
            else if (lowerQuery.includes('how many') || lowerQuery.includes('total') || lowerQuery.includes('count')) {
                aiResponse = `There are currently ${employees.length} employees registered in the system database.`;
            } 
            else if (lowerQuery.includes('department') || lowerQuery.includes('teams')) {
                const depts = Array.from(new Set(employees.map(e => e.department || 'Engineering')));
                aiResponse = `We have active personnel across the following departments: ${depts.join(', ')}.`;
            } 
            else if (lowerQuery.includes('salary') || lowerQuery.includes('pay') || lowerQuery.includes('average')) {
                if (employees.length === 0) {
                    aiResponse = "No employee salary data is currently loaded to calculate an average.";
                } else {
                    const totalSalary = employees.reduce((acc, curr) => acc + (curr.salary || 75000), 0);
                    const avgSalary = Math.round(totalSalary / employees.length);
                    aiResponse = `The average employee salary across the enterprise is approximately $${avgSalary.toLocaleString()}.`;
                }
            } 
            else {
                // Check if the user is searching for a specific employee name
                const matchedEmployee = employees.find(emp => 
                    lowerQuery.includes(emp.firstName.toLowerCase()) || 
                    lowerQuery.includes(emp.lastName.toLowerCase())
                );

                if (matchedEmployee) {
                    const pos = matchedEmployee.position || (matchedEmployee as any).jobTitle || 'Software Engineer';
                    aiResponse = `Found record: ${matchedEmployee.firstName} ${matchedEmployee.lastName} works as a ${pos} in the ${matchedEmployee.department || 'Engineering'} department. Email: ${matchedEmployee.email}.`;
                }
            }

            setMessages(prev => [...prev, { sender: 'ai', text: aiResponse }]);
        }, 600);
    };

    return (
        <div className="fixed bottom-6 right-6 z-50">
            {!isOpen ? (
                <button
                    onClick={() => setIsOpen(true)}
                    className="flex items-center space-x-2 bg-purple-600 hover:bg-purple-500 text-white px-4 py-3 rounded-full shadow-2xl transition transform hover:scale-105"
                >
                    <MessageSquare size={20} />
                    <span className="text-sm font-semibold">AI Assistant</span>
                </button>
            ) : (
                <div className="w-80 sm:w-96 bg-gray-900 border border-gray-800 rounded-2xl shadow-2xl flex flex-col h-[480px] overflow-hidden">
                    {/* Chat Header */}
                    <div className="flex items-center justify-between bg-gray-850 px-4 py-3 border-b border-gray-800">
                        <div className="flex items-center space-x-2">
                            <Bot size={18} className="text-purple-400" />
                            <span className="text-sm font-bold text-white flex items-center gap-1.5">
                                Enterprise AI <Sparkles size={12} className="text-purple-400" />
                            </span>
                        </div>
                        <button
                            onClick={() => setIsOpen(false)}
                            className="text-gray-400 hover:text-white transition"
                        >
                            <X size={18} />
                        </button>
                    </div>

                    {/* Chat Messages */}
                    <div className="flex-1 overflow-y-auto p-4 space-y-3 text-sm">
                        {messages.map((msg, index) => (
                            <div
                                key={index}
                                className={`flex items-start space-x-2 ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}
                            >
                                {msg.sender === 'ai' && (
                                    <div className="w-7 h-7 rounded-full bg-purple-950 border border-purple-800 flex items-center justify-center text-purple-300 shrink-0">
                                        <Bot size={14} />
                                    </div>
                                )}
                                <div
                                    className={`max-w-[78%] rounded-2xl px-3.5 py-2.5 leading-relaxed ${
                                        msg.sender === 'user'
                                            ? 'bg-purple-600 text-white rounded-br-none'
                                            : 'bg-gray-800 text-gray-200 border border-gray-700 rounded-bl-none'
                                    }`}
                                >
                                    {msg.text}
                                </div>
                                {msg.sender === 'user' && (
                                    <div className="w-7 h-7 rounded-full bg-gray-800 border border-gray-700 flex items-center justify-center text-gray-300 shrink-0">
                                        <User size={14} />
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>

                    {/* Chat Input Form */}
                    <form onSubmit={handleSend} className="p-3 border-t border-gray-800 bg-gray-900 flex items-center space-x-2">
                        <input
                            type="text"
                            placeholder="Ask about staff, salary, counts..."
                            value={input}
                            onChange={(e) => setInput(e.target.value)}
                            className="flex-1 bg-gray-800 border border-gray-700 rounded-xl px-3 py-2 text-sm text-white focus:outline-none focus:border-purple-500"
                        />
                        <button
                            type="submit"
                            className="bg-purple-600 hover:bg-purple-500 text-white p-2 rounded-xl transition"
                        >
                            <Send size={16} />
                        </button>
                    </form>
                </div>
            )}
        </div>
    );
};