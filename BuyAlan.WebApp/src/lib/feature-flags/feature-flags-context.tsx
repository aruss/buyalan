"use client";

import type { ReactElement, ReactNode } from "react";
import { createContext, useContext } from "react";
import type {
  FeatureFlagKey,
  FeatureFlagSnapshot,
} from "@/lib/feature-flags/feature-flag-registry";

const FeatureFlagsContext = createContext<FeatureFlagSnapshot | undefined>(
  undefined,
);

type FeatureFlagsProviderProps = {
  children: ReactNode;
  snapshot: FeatureFlagSnapshot;
};

export const FeatureFlagsProvider = ({
  children,
  snapshot,
}: FeatureFlagsProviderProps): ReactElement => {
  return (
    <FeatureFlagsContext.Provider value={snapshot}>
      {children}
    </FeatureFlagsContext.Provider>
  );
};

export const useFeatureFlags = (): FeatureFlagSnapshot => {
  const context = useContext(FeatureFlagsContext);

  if (context == null) {
    throw new Error(
      "useFeatureFlags must be used within FeatureFlagsProvider.",
    );
  }

  return context;
};

export const useFeatureFlag = (key: FeatureFlagKey): boolean => {
  const featureFlags = useFeatureFlags();

  return featureFlags[key];
};
