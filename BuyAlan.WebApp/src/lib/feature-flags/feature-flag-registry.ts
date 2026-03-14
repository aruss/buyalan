export type FeatureFlagDefinition<Key extends string = string> = {
  key: Key;
  defaultValue: boolean;
  description: string;
  owner?: string;
  category?: string;
};

const defineFeatureFlags = <
  Flags extends {
    [Key in keyof Flags]: FeatureFlagDefinition<Extract<Key, string>>;
  },
>(
  flags: Flags,
): Flags => {
  return flags;
};

const featureFlagRegistryDefinition = defineFeatureFlags({
  landingPricingEnabled: {
    key: "landingPricingEnabled",
    defaultValue: false,
    description:
      "Controls pricing visibility on the landing page, including pricing-related navigation.",
    category: "landing",
  },
});

export const featureFlagRegistry = featureFlagRegistryDefinition;

export type FeatureFlagKey = keyof typeof featureFlagRegistry;

export type FeatureFlagSnapshot = {
  [Key in FeatureFlagKey]: boolean;
};

export const featureFlagKeys = Object.keys(featureFlagRegistry) as FeatureFlagKey[];
