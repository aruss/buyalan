"use client";

import type { ReactElement, ReactNode } from "react";
import { createContext, useContext, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSession } from "@/lib/session-context";
import {
  getAgentsByAgentIdOptions,
  getAgentsByAgentIdQueryKey,
  getAgentsOptions,
  getAgentsQueryKey,
  postAgentsByAgentIdMutation,
} from "@/lib/api/@tanstack/react-query.gen";
import type {
  AgentPersonality,
  AgentResult,
  GetAgentsResponse,
  PostAgentInput,
} from "@/lib/api";

type UpdateProfileInput = {
  name: string | null;
  personality: AgentPersonality | null;
  personalityPromptRaw: string | null;
};

type UpdateChannelsInput = {
  twilioPhoneNumber: string | null;
  whatsappNumber: string | null;
  telegramBotToken: string | null;
};

type AgentSettingsContextValue = {
  agent: AgentResult | null;
  isLoading: boolean;
  isOperationalReady: boolean;
  errorMessage: string | null;
  refresh: () => Promise<void>;
  updateProfile: (input: UpdateProfileInput) => Promise<AgentResult | null>;
  updateChannels: (input: UpdateChannelsInput) => Promise<AgentResult | null>;
};

const DEFAULT_AGENT_SETTINGS_ERROR = "Unable to load agent settings.";
const NO_AGENT_SETTINGS_ERROR = "No agent found for this subscription.";
const MISSING_SUBSCRIPTION_ERROR = "No active subscription available for this account.";

const AgentSettingsContext = createContext<AgentSettingsContextValue | undefined>(undefined);

const resolveApiErrorMessage = (error: unknown, fallback: string): string => {
  if (error && typeof error === "object") {
    const errorRecord = error as Record<string, unknown>;
    const message = errorRecord.message;
    if (typeof message === "string" && message.trim().length > 0) {
      return message;
    }
  }

  return fallback;
};

const mergePostAgentInput = (
  currentAgent: AgentResult,
  input: Partial<PostAgentInput>,
): PostAgentInput => {
  return {
    name: input.name ?? currentAgent.name ?? null,
    personality: input.personality ?? currentAgent.personality ?? null,
    personalityPromptRaw: input.personalityPromptRaw ?? currentAgent.personalityPromptRaw ?? null,
    twilioPhoneNumber: input.twilioPhoneNumber ?? currentAgent.twilioPhoneNumber ?? null,
    whatsappNumber: input.whatsappNumber ?? currentAgent.whatsappNumber ?? null,
    telegramBotToken: input.telegramBotToken ?? currentAgent.telegramBotToken ?? null,
  };
};

const updateAgentsListCache = (
  current: GetAgentsResponse | undefined,
  updatedAgent: AgentResult,
): GetAgentsResponse | undefined => {
  if (!current?.items) {
    return current;
  }

  const nextItems = current.items.map((item) => {
    if (item.agentId !== updatedAgent.agentId) {
      return item;
    }

    return {
      ...item,
      name: updatedAgent.name,
      personality: updatedAgent.personality,
      isOperationalReady: updatedAgent.isOperationalReady,
      updatedAt: updatedAgent.updatedAt,
    };
  });

  return {
    ...current,
    items: nextItems,
  };
};

export const AgentSettingsProvider = ({
  children,
}: {
  children: ReactNode;
}): ReactElement => {
  const queryClient = useQueryClient();
  const { currentUser, isLoading: isSessionLoading, errorMessage: sessionErrorMessage } = useSession();
  const [mutationErrorMessage, setMutationErrorMessage] = useState<string | null>(null);

  const subscriptionId = currentUser?.activeSubscriptionId ?? null;

  const agentsQuery = useQuery({
    ...getAgentsOptions({
      query: {
        subscription: subscriptionId ?? "",
      },
    }),
    enabled: subscriptionId !== null,
    retry: false,
  });

  const agentId = agentsQuery.data?.items?.[0]?.agentId ?? null;

  const agentQuery = useQuery({
    ...getAgentsByAgentIdOptions({
      path: {
        agentId: agentId ?? "",
      },
    }),
    enabled: agentId !== null,
    retry: false,
  });

  const updateAgentMutation = useMutation(postAgentsByAgentIdMutation());

  const updateAgentAsync = async (input: Partial<PostAgentInput>): Promise<AgentResult | null> => {
    setMutationErrorMessage(null);

    const currentAgent = agentQuery.data ?? null;
    if (currentAgent === null) {
      setMutationErrorMessage(NO_AGENT_SETTINGS_ERROR);
      return null;
    }

    try {
      const updatedAgent = await updateAgentMutation.mutateAsync({
        path: { agentId: currentAgent.agentId },
        body: mergePostAgentInput(currentAgent, input),
      });

      queryClient.setQueryData(
        getAgentsByAgentIdQueryKey({ path: { agentId: updatedAgent.agentId } }),
        updatedAgent,
      );

      if (subscriptionId !== null) {
        queryClient.setQueryData<GetAgentsResponse | undefined>(
          getAgentsQueryKey({
            query: { subscription: subscriptionId },
          }),
          (current) => updateAgentsListCache(current, updatedAgent),
        );
      }

      return updatedAgent;
    } catch (error: unknown) {
      setMutationErrorMessage(resolveApiErrorMessage(error, DEFAULT_AGENT_SETTINGS_ERROR));
      return null;
    }
  };

  const updateProfile = async (input: UpdateProfileInput): Promise<AgentResult | null> => {
    return updateAgentAsync({
      name: input.name,
      personality: input.personality,
      personalityPromptRaw: input.personalityPromptRaw,
    });
  };

  const updateChannels = async (input: UpdateChannelsInput): Promise<AgentResult | null> => {
    return updateAgentAsync({
      twilioPhoneNumber: input.twilioPhoneNumber,
      whatsappNumber: input.whatsappNumber,
      telegramBotToken: input.telegramBotToken,
    });
  };

  const refresh = async (): Promise<void> => {
    setMutationErrorMessage(null);
    await agentsQuery.refetch();
    await agentQuery.refetch();
  };

  const isLoading =
    isSessionLoading ||
    agentsQuery.isLoading ||
    agentsQuery.isRefetching ||
    agentQuery.isLoading ||
    agentQuery.isRefetching ||
    updateAgentMutation.isPending;

  const errorMessage = useMemo((): string | null => {
    if (sessionErrorMessage) {
      return sessionErrorMessage;
    }

    if (subscriptionId === null) {
      return MISSING_SUBSCRIPTION_ERROR;
    }

    if (mutationErrorMessage) {
      return mutationErrorMessage;
    }

    if (agentsQuery.error || agentQuery.error) {
      return DEFAULT_AGENT_SETTINGS_ERROR;
    }

    if (!isLoading && agentId === null) {
      return NO_AGENT_SETTINGS_ERROR;
    }

    return null;
  }, [
    agentId,
    agentQuery.error,
    agentsQuery.error,
    isLoading,
    mutationErrorMessage,
    sessionErrorMessage,
    subscriptionId,
  ]);

  const agent = agentQuery.data ?? null;

  const value: AgentSettingsContextValue = {
    agent,
    isLoading,
    isOperationalReady: agent?.isOperationalReady ?? false,
    errorMessage,
    refresh,
    updateProfile,
    updateChannels,
  };

  return <AgentSettingsContext.Provider value={value}>{children}</AgentSettingsContext.Provider>;
};

export const useAgentSettings = (): AgentSettingsContextValue => {
  const context = useContext(AgentSettingsContext);
  if (!context) {
    throw new Error("useAgentSettings must be used within AgentSettingsProvider.");
  }

  return context;
};
