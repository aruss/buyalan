"use client";

import { ReactElement, Suspense, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { postNewsletterConfirm } from "@/lib/api";
import { NEWSLETTER_CONFIRMATION_COOKIE_NAME } from "@/components/landing/newsletter-constants";

const REDIRECT_DELAY_MS = 2500;

function markNewsletterCookie(): void {
    const secureFlag = window.location.protocol === "https:" ? "; secure" : "";
    document.cookie = `${NEWSLETTER_CONFIRMATION_COOKIE_NAME}=1; path=/; samesite=lax${secureFlag}`;
}

async function confirmSubscriptionAsync(token: string): Promise<void> {
    await postNewsletterConfirm({
        body: {
            token
        },
        throwOnError: true
    });
}

function NewsletterConfirmPageContent(): ReactElement {
    const router = useRouter();
    const searchParams = useSearchParams();

    useEffect(() => {
        const token = searchParams.get("token");

        const runConfirmationAsync = async (): Promise<void> => {
            if (token !== null && token.trim().length > 0) {
                try {
                    await confirmSubscriptionAsync(token);
                    markNewsletterCookie();
                } catch {
                    // Keep UX generic to avoid token state disclosure.
                }
            }

            window.setTimeout(() => {
                router.replace("/");
            }, REDIRECT_DELAY_MS);
        };

        void runConfirmationAsync();
    }, [router, searchParams]);

    return (
        <main className="mx-auto flex min-h-[60vh] max-w-3xl items-center justify-center px-6 py-24">
            <div className="rounded-lg border border-zinc-200 bg-white p-8 text-center shadow-sm">
                <h1 className="text-2xl font-semibold tracking-tight text-zinc-900">
                    Thank you for subscribing to our newsletter.
                </h1>
                <p className="mt-3 text-sm text-zinc-600">
                    You are being redirected to the landing page...
                </p>
            </div>
        </main>
    );
}

function NewsletterConfirmFallback(): ReactElement {
    return (
        <main className="mx-auto flex min-h-[60vh] max-w-3xl items-center justify-center px-6 py-24">
            <div className="rounded-lg border border-zinc-200 bg-white p-8 text-center shadow-sm" aria-hidden="true">
                <h1 className="text-2xl font-semibold tracking-tight text-zinc-900">
                    Thank you for subscribing to our newsletter.
                </h1>
                <p className="mt-3 text-sm text-zinc-600">
                    You are being redirected to the landing page...
                </p>
            </div>
        </main>
    );
}

export default function NewsletterConfirmPage(): ReactElement {
    return (
        <Suspense fallback={<NewsletterConfirmFallback />}>
            <NewsletterConfirmPageContent />
        </Suspense>
    );
}
