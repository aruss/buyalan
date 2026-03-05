import { cookies } from "next/headers";
import Link from "next/link";
import { ReactElement } from "react";
import { NEWSLETTER_CONFIRMATION_COOKIE_NAME } from "./newsletter-constants";
import { NewsletterSubscriptionForm } from "./newsletter-subscription-form";

export const LandingFooter = async (): Promise<ReactElement> => {
    const cookieStore = await cookies();
    const isSubscribedForSession = cookieStore.get(NEWSLETTER_CONFIRMATION_COOKIE_NAME)?.value === "1";

    return (
        <footer className="border-t border-zinc-800 bg-black py-16 text-white">
            <div className="mx-auto grid max-w-7xl grid-cols-1 gap-12 px-6 md:grid-cols-4">
                <div className="md:col-span-2">
                    <Link href="/" className="mb-4 text-xl font-bold tracking-tight">
                        <span className="text-zinc-500">Hey</span>Alan
                    </Link>
                    <p className="mb-6 max-w-sm text-sm text-zinc-500">
                        Empowering Square merchants with autonomous conversational sales.
                    </p>
                    <NewsletterSubscriptionForm isInitiallySubmitted={isSubscribedForSession} />
                </div>

                <div>
                    <h4 className="mb-4 text-sm font-semibold tracking-wider text-zinc-400 uppercase">Platform</h4>
                    <ul className="space-y-3 text-sm text-zinc-500">
                        <li>
                            <Link href="#features" className="transition-colors hover:text-white">
                                Features
                            </Link>
                        </li>
                        <li>
                            <Link href="#dashboard" className="transition-colors hover:text-white">
                                Merchant Dashboard
                            </Link>
                        </li>
                        <li>
                            <Link href="#pricing" className="transition-colors hover:text-white">
                                Pricing
                            </Link>
                        </li>
                    </ul>
                </div>

                <div>
                    <h4 className="mb-4 text-sm font-semibold tracking-wider text-zinc-400 uppercase">Legal</h4>
                    <ul className="space-y-3 text-sm text-zinc-500">
                        <li>
                            <Link href="/terms" className="transition-colors hover:text-white">
                                Terms of Service
                            </Link>
                        </li>
                        <li>
                            <Link href="/privacy" className="transition-colors hover:text-white">
                                Privacy Policy
                            </Link>
                        </li>
                        <li>
                            <Link href="/privacy#cookies" className="transition-colors hover:text-white">
                                Cookie Policy
                            </Link>
                        </li>
                        <li>
                            <Link href="/imprint" className="transition-colors hover:text-white">
                                Imprint
                            </Link>
                        </li>
                    </ul>
                </div>
            </div>
            <div className="mx-auto mt-16 flex max-w-7xl flex-col items-center justify-between border-t border-zinc-900 px-6 pt-8 md:flex-row">
                <p className="text-xs text-zinc-600">(c) 2026 HeyAlan. All rights reserved.</p>
                <p className="mt-2 text-xs text-zinc-600 md:mt-0">Not affiliated with Block, Inc.</p>
            </div>
        </footer>
    );
};
