namespace Application.DTOs.CalendarDtos;

public record CalendarEventDto(
    string Title,
    DateTime Start,
    DateTime End,
    string Location,
    string Description
);
