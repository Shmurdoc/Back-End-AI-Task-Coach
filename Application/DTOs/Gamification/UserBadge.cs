namespace Application.DTOs.Gamification;

public record UserBadge(
    Guid Id,
    string Name,
    string Description,
    string IconUrl,
    DateTime EarnedAt,
    string Reason,
    BadgeRarity Rarity = BadgeRarity.Common
);

public enum BadgeRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
