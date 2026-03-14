import "server-only";

import {
  featureFlagKeys,
  type FeatureFlagKey,
  type FeatureFlagSnapshot,
} from "@/lib/feature-flags/feature-flag-registry";
import {
  resolveFeatureFlagValue,
  type FeatureFlagEnvironment,
} from "@/lib/feature-flags/feature-flag-env";

export const getFeatureFlagSnapshot = (
  environment: FeatureFlagEnvironment = process.env,
): FeatureFlagSnapshot => {
  return featureFlagKeys.reduce<FeatureFlagSnapshot>((snapshot, key) => {
    snapshot[key] = resolveFeatureFlagValue(key, environment);

    return snapshot;
  }, {} as FeatureFlagSnapshot);
};

export const isFeatureEnabled = (
  key: FeatureFlagKey,
  environment: FeatureFlagEnvironment = process.env,
): boolean => {
  const snapshot = getFeatureFlagSnapshot(environment);

  return snapshot[key];
};
