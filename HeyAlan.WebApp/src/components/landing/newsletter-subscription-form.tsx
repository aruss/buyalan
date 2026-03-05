"use client";

import type { ReactElement } from "react";
import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { postNewsletterSubscriptions } from "@/lib/api";
import { NEWSLETTER_CONFIRMATION_COOKIE_NAME } from "./newsletter-constants";

type NewsletterFormValues = {
    email: string;
};

const newsletterSchema = z.object({
    email: z.string().trim().refine((value) => {
        return z.email().safeParse(value).success;
    }, "Use a valid email format.")
});

type NewsletterSubscriptionFormProps = {
    isInitiallySubmitted: boolean;
};

export const NewsletterSubscriptionForm = ({
    isInitiallySubmitted
}: NewsletterSubscriptionFormProps): ReactElement => {
    const [isSubmitted, setIsSubmitted] = useState<boolean>(isInitiallySubmitted);
    const [submitError, setSubmitError] = useState<string | null>(null);

    const form = useForm<NewsletterFormValues>({
        resolver: zodResolver(newsletterSchema),
        defaultValues: {
            email: ""
        }
    });

    const onSubmitAsync = async (values: NewsletterFormValues): Promise<void> => {
        setSubmitError(null);

        try {
            await postNewsletterSubscriptions({
                body: {
                    email: values.email.trim()
                },
                throwOnError: true
            });
        } catch {
            setSubmitError("Unable to process your request right now. Please try again.");
            return;
        }

        const secureFlag = window.location.protocol === "https:" ? "; secure" : "";
        document.cookie = `${NEWSLETTER_CONFIRMATION_COOKIE_NAME}=1; path=/; samesite=lax${secureFlag}`;
        setIsSubmitted(true);
    };

    if (isSubmitted) {
        return (
            <p className="max-w-sm rounded border border-zinc-800 bg-zinc-900 px-4 py-3 text-sm text-zinc-300">
                You are subscribed. Please check your inbox to confirm.
            </p>
        );
    }

    const emailError = form.formState.errors.email?.message;

    return (
        <form
            className="flex max-w-md flex-col gap-2"
            onSubmit={form.handleSubmit((values) => {
                void onSubmitAsync(values);
            })}
            noValidate
        >
            <div className="flex flex-col gap-2 sm:flex-row">
                <input
                    type="email"
                    placeholder="Subscribe to newsletter"
                    className="flex-1 rounded border border-zinc-800 bg-zinc-900 px-4 py-2.5 text-sm text-white focus:border-zinc-500 focus:outline-none"
                    {...form.register("email")}
                />
                <button
                    type="submit"
                    disabled={form.formState.isSubmitting}
                    className="rounded bg-white px-6 py-2.5 text-sm font-semibold text-black transition-colors hover:bg-zinc-200 disabled:cursor-not-allowed disabled:opacity-70"
                >
                    {form.formState.isSubmitting ? "Submitting..." : "Subscribe"}
                </button>
            </div>
            {emailError ? (
                <p className="text-xs text-red-400">{emailError}</p>
            ) : null}
            {submitError ? (
                <p className="text-xs text-red-400">{submitError}</p>
            ) : null}
        </form>
    );
};
