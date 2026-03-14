import type { Metadata } from "next";
import type { ReactElement, ReactNode } from "react";
import "../globals.css";
import { LandingNavigation } from "@/components/landing/landing-navigation";
import { LandingFooter } from "@/components/landing/landing-footer";
import { FeatureFlagsProvider } from "@/lib/feature-flags";
import { getFeatureFlagSnapshot } from "@/lib/feature-flags/server";

export const metadata: Metadata = {
    title: "BuyAlan",
    description: "AI Sales Agent for Square",
};

export default async function RootLayout({
    children,
}: Readonly<{
    children: ReactNode;
}>): Promise<ReactElement> {
    const featureFlagSnapshot = getFeatureFlagSnapshot();

    return (
        <html lang="en">
            <body>
                <FeatureFlagsProvider snapshot={featureFlagSnapshot}>
                    <div className="bg-white text-zinc-900 antialiased smooth-scroll selection:bg-zinc-900 selection:text-white">
                        <LandingNavigation />
                        <main>{children}</main>
                        <LandingFooter />
                    </div>
                </FeatureFlagsProvider>
            </body>
        </html>
    );
}
