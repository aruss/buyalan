import { ReactElement } from "react";
import { LuCircleCheck } from "react-icons/lu";

export const LandingDashboard = (): ReactElement => {
    return (
        <section id="dashboard" className="overflow-hidden py-24">
            <div className="mx-auto max-w-7xl px-4 md:px-6">
                <div className="flex flex-col items-center gap-16 lg:flex-row">
                    <div className="w-full lg:w-1/2">
                        <h2 className="mb-6 text-3xl font-bold tracking-tight md:text-5xl">Total merchant control.</h2>
                        <p className="mb-10 text-xl font-light text-zinc-600">
                            A centralized command center designed for oversight, intervention, and analytics.
                        </p>

                        <ul className="space-y-8">
                            <li className="flex items-start">
                                <div className="mt-1 shrink-0">
                                    <LuCircleCheck className="h-5 w-5 text-zinc-900" aria-hidden="true" />
                                </div>
                                <div className="ml-4">
                                    <h3 className="text-lg font-semibold">Human Takeover Protocol</h3>
                                    <p className="mt-1 text-sm text-zinc-600">
                                        Monitor live conversations. Instantly pause the AI agent to engage the customer
                                        manually, and hand back control when finished.
                                    </p>
                                </div>
                            </li>
                            <li className="flex items-start">
                                <div className="mt-1 shrink-0">
                                    <LuCircleCheck className="h-5 w-5 text-zinc-900" aria-hidden="true" />
                                </div>
                                <div className="ml-4">
                                    <h3 className="text-lg font-semibold">Conversation-to-Order Attribution</h3>
                                    <p className="mt-1 text-sm text-zinc-600">
                                        View list of all chats directly linked to resulting orders. See live order state
                                        and historical chat logs side-by-side.
                                    </p>
                                </div>
                            </li>
                            <li className="flex items-start">
                                <div className="mt-1 shrink-0">
                                    <LuCircleCheck className="h-5 w-5 text-zinc-900" aria-hidden="true" />
                                </div>
                                <div className="ml-4">
                                    <h3 className="text-lg font-semibold">Behavioral Tuning</h3>
                                    <p className="mt-1 text-sm text-zinc-600">
                                        Adjust agent parameters. Modify tone, set aggressive or conservative upsell
                                        triggers, and define fallback rules.
                                    </p>
                                </div>
                            </li>
                        </ul>
                    </div>

                    <div className="w-full lg:w-1/2">
                        <div className="transform rounded-2xl border border-zinc-200 bg-zinc-50 p-4 shadow-2xl transition-transform duration-500 hover:rotate-0 md:p-6 lg:rotate-1">
                            <div className="mb-4 flex items-center justify-between border-b border-zinc-200 pb-4">
                                <div className="flex space-x-2">
                                    <div className="h-3 w-3 rounded-full bg-red-400"></div>
                                    <div className="h-3 w-3 rounded-full bg-yellow-400"></div>
                                    <div className="h-3 w-3 rounded-full bg-green-400"></div>
                                </div>
                                <div className="text-xs font-semibold tracking-wider text-zinc-600 uppercase">
                                    Active Sessions
                                </div>
                            </div>
                            <div className="flex h-96 gap-4">
                                <div className="w-1/3 border-r border-zinc-200 pr-4">
                                    <div className="flex flex-col gap-2">
                                        <div className="rounded border border-zinc-200 border-l-4 border-l-green-500 bg-white p-3 shadow-sm">
                                            <div className="text-xs font-bold">User #8492</div>
                                            <div className="truncate text-[10px] text-zinc-700">WhatsApp - Browsing</div>
                                        </div>
                                        <div className="rounded border border-zinc-200 bg-zinc-100 p-3">
                                            <div className="text-xs font-bold text-zinc-900">User #1023</div>
                                            <div className="truncate text-[10px] text-zinc-800">
                                                SMS - Checkout Pending
                                            </div>
                                        </div>
                                        <div className="rounded border border-zinc-200 bg-zinc-100 p-3">
                                            <div className="text-xs font-bold text-zinc-900">User #5521</div>
                                            <div className="truncate text-[10px] text-zinc-800">Telegram - Support</div>
                                        </div>
                                    </div>
                                </div>
                                <div className="relative flex w-2/3 flex-col">
                                    <div className="absolute top-0 right-0 cursor-pointer rounded border border-red-200 bg-red-100 px-2 py-1 text-xs font-medium text-red-700">
                                        Takeover Chat
                                    </div>
                                    <div className="mt-8 flex-1 space-y-3">
                                        <div className="h-8 w-3/4 rounded-lg rounded-tl-none bg-zinc-100"></div>
                                        <div className="ml-auto h-8 w-1/2 rounded-lg rounded-tr-none bg-zinc-900"></div>
                                        <div className="h-12 w-5/6 rounded-lg rounded-tl-none bg-zinc-100"></div>

                                        <div className="mt-4 rounded border border-zinc-200 bg-white p-3 text-xs">
                                            <div className="mb-2 border-b border-zinc-100 pb-1 font-bold">Order #SQ-902</div>
                                            <div className="flex justify-between text-zinc-700">
                                                <span>Ceramic Vase (x1)</span>
                                                <span>$45.00</span>
                                            </div>
                                            <div className="flex justify-between text-zinc-700">
                                                <span>Express Ship</span>
                                                <span>$10.00</span>
                                            </div>
                                            <div className="mt-2 flex justify-between border-t border-zinc-100 pt-2 font-bold">
                                                <span>Total</span>
                                                <span>$55.00</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
};
