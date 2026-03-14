"use client";

import Link from "next/link";
import { useRef, useState, type ReactElement } from "react";
import { LuMenu } from "react-icons/lu";
import { PrimaryActionButton } from "@/components/landing/ui/action-buttons";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/landing/ui/dropdown-menu";

export const LandingNavigation = (): ReactElement => {
    const [isOpen, setIsOpen] = useState(false);
    const triggerRef = useRef<HTMLButtonElement | null>(null);

    const handleOpenChange = (open: boolean): void => {
        setIsOpen(open);

        if (!open) {
            window.setTimeout(() => {
                triggerRef.current?.focus();
            }, 0);
        }
    };

    const closeMenu = (): void => {
        handleOpenChange(false);
    };

    return (
        <nav className="fixed top-0 z-50 w-full border-b border-zinc-100 bg-white/80 backdrop-blur-md">
            <div className="mx-auto flex h-20 max-w-7xl items-center justify-between px-6">
                <Link href="/" className="text-xl font-bold tracking-tight">
                    <span className="text-zinc-500">Buy</span>Alan
                </Link>
                <div className="hidden space-x-8 text-sm font-medium md:flex">
                    <Link href="#features" className="transition-colors hover:text-zinc-500">
                        Features
                    </Link>
                    <Link href="#dashboard" className="transition-colors hover:text-zinc-500">
                        Merchant Dashboard
                    </Link>
                    <Link href="#pricing" className="transition-colors hover:text-zinc-500">
                        Pricing
                    </Link>
                    <Link href="#compliance" className="transition-colors hover:text-zinc-500">
                        Trust
                    </Link>
                </div>
                <div className="flex items-center space-x-4">
                    <Link href="/admin" className="hidden text-sm font-medium transition-colors hover:text-zinc-500 md:block">
                        Log In
                    </Link>
                    <PrimaryActionButton href="/admin" size="sm" className="hidden md:inline-flex">
                        Start Free Trial
                    </PrimaryActionButton>
                    <DropdownMenu open={isOpen} onOpenChange={handleOpenChange}>
                        <DropdownMenuTrigger asChild>
                            <button
                                type="button"
                                ref={triggerRef}
                                className="inline-flex items-center justify-center rounded-xl border border-zinc-200 p-2 text-zinc-900 transition-colors hover:bg-zinc-50 md:hidden"
                                aria-label="Open navigation menu"
                            >
                                <LuMenu className="h-5 w-5" aria-hidden="true" />
                            </button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent
                            align="end"
                            className="w-64 md:hidden"
                            onCloseAutoFocus={(event) => {
                                event.preventDefault();
                                triggerRef.current?.focus();
                            }}
                        >
                            <DropdownMenuItem asChild onSelect={closeMenu}>
                                <Link href="#features">Features</Link>
                            </DropdownMenuItem>
                            <DropdownMenuItem asChild onSelect={closeMenu}>
                                <Link href="#dashboard">Merchant Dashboard</Link>
                            </DropdownMenuItem>
                            <DropdownMenuItem asChild onSelect={closeMenu}>
                                <Link href="#pricing">Pricing</Link>
                            </DropdownMenuItem>
                            <DropdownMenuItem asChild onSelect={closeMenu}>
                                <Link href="#compliance">Trust</Link>
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem asChild onSelect={closeMenu}>
                                <Link href="/admin">Start Free Trial</Link>
                            </DropdownMenuItem>
                        </DropdownMenuContent>
                    </DropdownMenu>
                </div>
            </div>
        </nav>
    );
};
