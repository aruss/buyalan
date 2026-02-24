import React, { useState } from 'react';
import { 
  MessageSquare, Bell, UserCircle, Search, 
  Bot, User, ShoppingCart, Package, CreditCard, 
  ChevronRight, Send, AlertCircle, Phone, Lock
} from 'lucide-react';

// Mock Data
const CONVERSATIONS = [
  { id: '1', name: '+1 555-0101', channel: 'WhatsApp', lastMsg: 'Is the Pro version in stock?', time: '10:42 AM', unread: true },
  { id: '2', name: '@alex_dev', channel: 'Telegram', lastMsg: 'Payment completed.', time: '09:15 AM', unread: false },
  { id: '3', name: '+44 7700 900077', channel: 'SMS', lastMsg: 'When will it ship?', time: 'Yesterday', unread: false },
];

const MESSAGES = [
  { id: 'm1', sender: 'user', text: 'Hi, I want to order the mechanical keyboard.', time: '10:40 AM' },
  { id: 'm2', sender: 'agent', text: 'Hello! I can help with that. Are you looking for the tactile or linear switches?', time: '10:40 AM' },
  { id: 'm3', sender: 'user', text: 'Tactile, please. Is the Pro version in stock?', time: '10:42 AM' },
];

const CART = [
  { id: 'c1', name: 'Mech Keyboard Pro', variant: 'Tactile / Black', price: 149.99, qty: 1 }
];

const PAST_ORDERS = [
  { id: 'ORD-882', date: 'Oct 12, 2025', total: 45.00, status: 'Delivered' },
  { id: 'ORD-751', date: 'Aug 04, 2025', total: 120.50, status: 'Delivered' }
];

export default function App() {
  const [activeChat, setActiveChat] = useState(CONVERSATIONS[0].id);
  const [searchQuery, setSearchQuery] = useState('');
  const [agentActive, setAgentActive] = useState(true);

  const currentChat = CONVERSATIONS.find(c => c.id === activeChat) || CONVERSATIONS[0];

  return (
    <div className="flex h-screen w-full overflow-hidden bg-slate-50 font-sans text-slate-900">
      
      {/* Main Content Area */}
      <main className="flex min-w-0 flex-1 flex-col bg-slate-50">
        
        {/* 3-Column Dashboard */}
        <div className="flex flex-1 overflow-hidden">
          
          {/* Column 1: Chat List */}
          <section className="flex w-80 shrink-0 flex-col border-r border-slate-200 bg-white">
            <div className="border-b border-slate-200 p-3">
              <h2 className="mb-3 text-lg font-medium text-slate-900">Messages</h2>
              <div className="relative flex items-center">
                <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
                <input 
                  type="text"
                  placeholder="Search conversations..." 
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full rounded-md border border-slate-200 py-2 pl-9 pr-3 text-sm focus:border-slate-900 focus:outline-none focus:ring-1 focus:ring-slate-900"
                />
              </div>
            </div>
            <div className="flex-1 overflow-y-auto">
              {CONVERSATIONS.map((chat) => (
                <div 
                  key={chat.id}
                  onClick={() => setActiveChat(chat.id)}
                  className={`w-full cursor-pointer border-b border-slate-100 px-3 py-2 text-left transition-colors hover:bg-slate-50 ${activeChat === chat.id ? 'bg-slate-50' : ''}`}
                >
                  <div className="flex items-baseline justify-between">
                    <span className="truncate pr-2 text-sm font-medium text-slate-900">{chat.name}</span>
                    <span className="shrink-0 text-[10px] text-slate-500">{chat.time}</span>
                  </div>
                  <div className="mt-0.5 flex items-center justify-between">
                    <span className="truncate pr-2 text-xs text-slate-500">{chat.lastMsg}</span>
                    <div className="flex shrink-0 items-center space-x-2">
                      {chat.unread && <span className="size-1.5 rounded-full bg-slate-900"></span>}
                      <span className="inline-flex items-center rounded-md bg-slate-100 px-1.5 py-0.5 text-[10px] font-medium text-slate-700">
                        {chat.channel}
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </section>

          {/* Column 2: Conversation Window */}
          <section className="flex min-w-0 flex-1 flex-col bg-white">
            {/* Chat Header */}
            <div className="flex h-14 shrink-0 items-center justify-between border-b border-slate-200 bg-slate-50 px-6">
              <div className="flex items-center space-x-3">
                <div className="mr-2 flex items-center space-x-2">
                  {currentChat.channel === 'WhatsApp' && <MessageSquare className="size-4 text-emerald-500" />}
                  {currentChat.channel === 'Telegram' && <Send className="size-4 text-blue-500" />}
                  {currentChat.channel === 'SMS' && <Phone className="size-4 text-slate-500" />}
                  <span className="text-sm font-semibold text-slate-900">{currentChat.name}</span>
                </div>
                <div className="mx-1 h-4 w-px bg-slate-300"></div>
                <span className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ${agentActive ? 'bg-slate-100 text-slate-700' : 'bg-gray-100 text-gray-700'}`}>
                  {agentActive ? <Bot className="mr-1 size-3" /> : <UserCircle className="mr-1 size-3" />}
                  {agentActive ? 'AI Agent Active' : 'Human Operator'}
                </span>
              </div>
              <button 
                onClick={() => setAgentActive(!agentActive)}
                className={`inline-flex items-center justify-center rounded-md px-2.5 py-1.5 text-xs font-medium transition-colors ${agentActive ? 'bg-slate-900 text-white hover:bg-slate-800' : 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'}`}
              >
                {agentActive ? 'Take Over' : 'Return to AI'}
              </button>
            </div>

            {/* Chat Messages */}
            <div className="flex-1 space-y-6 overflow-y-auto p-6">
              <div className="my-6 flex w-full items-center">
                <div className="grow border-t border-slate-200"></div>
                <span className="shrink-0 px-4 text-xs font-medium uppercase tracking-wider text-slate-400">Today</span>
                <div className="grow border-t border-slate-200"></div>
              </div>
              
              {MESSAGES.map((msg) => (
                <div key={msg.id} className={`flex ${msg.sender === 'user' ? 'justify-end' : 'justify-start'}`}>
                  <div className={`flex max-w-[70%] flex-col ${msg.sender === 'user' ? 'items-end' : 'items-start'}`}>
                    <div className="mb-1 flex items-center space-x-2">
                      <span className="block text-xs text-slate-400">{msg.time}</span>
                      {msg.sender === 'agent' && <Bot className="size-3 text-slate-400" />}
                    </div>
                    <div className={`rounded-xl p-3 shadow-sm ${msg.sender === 'user' ? 'rounded-tr-none bg-slate-900 text-slate-50' : 'rounded-tl-none border border-slate-200 bg-slate-100 text-slate-900'}`}>
                      <span className="block">{msg.text}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Chat Input */}
            <div className="flex shrink-0 items-center space-x-2 border-t border-slate-200 bg-white p-4">
              <div className="relative flex flex-1 items-center">
                <input 
                  type="text"
                  placeholder={agentActive ? "Take over to type a message..." : "Type your message..."}
                  disabled={agentActive}
                  className="w-full rounded-md border border-slate-200 py-2 pl-3 pr-3 text-sm focus:border-slate-900 focus:outline-none focus:ring-1 focus:ring-slate-900 disabled:opacity-50"
                />
              </div>
              <button 
                disabled={agentActive}
                className="inline-flex items-center justify-center rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-slate-800 disabled:opacity-50"
              >
                <Send className="mr-2 size-4" /> Send
              </button>
            </div>
          </section>

          {/* Column 3: Context & Actions */}
          <section className="flex w-80 shrink-0 flex-col space-y-4 border-l border-slate-200 bg-slate-50 p-4 overflow-y-auto">
            
            {/* Customer Info Card */}
            <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
              <span className="mb-4 flex items-center text-xs font-bold uppercase tracking-wider text-slate-400">
                <User className="mr-2 size-3.5" /> Customer Info
              </span>
              <div className="space-y-3">
                <div>
                  <span className="block font-medium text-slate-900">Unknown User</span>
                  <span className="block text-sm text-slate-500">+1 555-0101</span>
                </div>
                <div className="flex items-center justify-between text-sm">
                  <span className="block text-slate-500">LTV</span>
                  <span className="block font-medium text-slate-900">$165.50</span>
                </div>
                <div className="flex flex-wrap gap-2 pt-1">
                  <span className="inline-flex items-center rounded-md bg-slate-100 px-1.5 py-0.5 text-[10px] font-medium text-slate-700">Returning</span>
                  <span className="inline-flex items-center rounded-md bg-slate-100 px-1.5 py-0.5 text-[10px] font-medium text-slate-700">Tech</span>
                </div>
              </div>
            </div>

            {/* Current Cart / Order Context */}
            <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
              <span className="mb-4 flex items-center text-xs font-bold uppercase tracking-wider text-slate-400">
                <ShoppingCart className="mr-2 size-3.5" /> Active Cart
              </span>
              {CART.length > 0 ? (
                <>
                  <ul className="divide-y divide-slate-100">
                    {CART.map(item => (
                      <li key={item.id} className="flex items-center justify-between py-3">
                        <div>
                          <span className="block font-medium text-slate-900">{item.name}</span>
                          <span className="block text-xs text-slate-500">{item.variant} &times; {item.qty}</span>
                        </div>
                        <span className="block font-medium text-slate-900">${item.price}</span>
                      </li>
                    ))}
                  </ul>
                  <div className="my-6 flex w-full items-center">
                    <div className="grow border-t border-slate-200"></div>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="block font-semibold text-slate-900">Total</span>
                    <span className="block font-semibold text-slate-900">${CART.reduce((acc, curr) => acc + curr.price, 0).toFixed(2)}</span>
                  </div>
                </>
              ) : (
                <span className="block text-sm italic text-slate-500">Cart is empty.</span>
              )}
            </div>

            {/* Quick Actions */}
            <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
              <span className="mb-4 flex items-center text-xs font-bold uppercase tracking-wider text-slate-400">
                <Lock className="mr-2 size-3.5" /> Manual Actions
              </span>
              <div className="space-y-2">
                <button className="inline-flex w-full items-center justify-between rounded-md border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50">
                  <span className="flex items-center"><AlertCircle className="mr-2 size-4 text-slate-400" /> Trigger Upsell</span>
                </button>
                <button className="inline-flex w-full items-center justify-between rounded-md border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50">
                  <span className="flex items-center"><CreditCard className="mr-2 size-4 text-slate-400" /> Send Payment Link</span>
                </button>
                <button className="inline-flex w-full items-center justify-between rounded-md border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-50">
                  <span className="flex items-center"><Package className="mr-2 size-4 text-slate-400" /> Schedule Shipping</span>
                </button>
              </div>
            </div>

            {/* Past Orders */}
            <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
              <span className="mb-4 flex items-center text-xs font-bold uppercase tracking-wider text-slate-400">
                <Phone className="mr-2 size-3.5" /> Past Orders
              </span>
              <ul className="divide-y divide-slate-100">
                {PAST_ORDERS.map(order => (
                  <li key={order.id} className="flex flex-col items-start justify-between space-y-1 py-3">
                    <div className="flex w-full justify-between">
                      <span className="block text-xs font-semibold text-slate-900">{order.id}</span>
                      <span className="block text-xs text-slate-500">{order.date}</span>
                    </div>
                    <div className="flex w-full justify-between">
                      <span className="block text-xs font-medium text-slate-600">${order.total.toFixed(2)}</span>
                      <span className="block text-[10px] font-semibold uppercase text-slate-500">{order.status}</span>
                    </div>
                  </li>
                ))}
              </ul>
            </div>

          </section>
        </div>
      </main>
    </div>
  );
}