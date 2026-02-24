import type { Metadata } from "next";
import type { ReactElement, ReactNode } from "react";
import "./globals.css";

export const metadata: Metadata = {
    title: "SquareBuddy",
    description: "AI Sales Agent for Squarespace",
};

export default function RootLayout({
    children,
}: Readonly<{
    children: ReactNode;
}>): ReactElement {
    return (
        <html lang="en">
            <body>{children}</body>
        </html>
    );
}
