import type { Metadata } from "next";
import type { ReactElement, ReactNode } from "react";
import "../globals.css";
import { LandingNavigation } from "@/components/landing/landing-navigation";
import { LandingFooter } from "@/components/landing/landing-footer";

export const metadata: Metadata = {
    title: "BuyAlan",
    description: "AI Sales Agent for Square",
};

export default function RootLayout({
    children,
}: Readonly<{
    children: ReactNode;
}>): ReactElement {
    return (
        <html lang="en">
            <body>
                <div className="bg-white text-zinc-900 antialiased smooth-scroll selection:bg-zinc-900 selection:text-white">
                    <LandingNavigation />
                    <main>{children}</main>
                    <LandingFooter />
                </div>
            </body>
        </html>
    );
}
