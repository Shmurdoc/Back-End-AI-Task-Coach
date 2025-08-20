using AutoMapper;
using Domain.Entities;
using Application.DTOs.TaskDtos;
using Application.DTOs.HabitDtos;
using Application.DTOs.GoalDtos;

namespace Application.Mappers.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // TaskItem <-> TaskDto
            CreateMap<TaskItem, TaskDto>().ReverseMap();

            // CreateTaskDto -> TaskItem
            CreateMap<CreateTaskDto, TaskItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ActualHours, opt => opt.Ignore())
                .ForMember(dest => dest.CompletionPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore());

            // UpdateTaskDto -> TaskItem (for patching)
            CreateMap<UpdateTaskDto, TaskItem>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Habit <-> HabitDto
            CreateMap<Habit, HabitDto>().ReverseMap();

            // CreateHabitDto -> Habit
            CreateMap<CreateHabitDto, Habit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentStreak, opt => opt.Ignore())
                .ForMember(dest => dest.BestStreak, opt => opt.Ignore())
                .ForMember(dest => dest.CompletionRate, opt => opt.Ignore())
                .ForMember(dest => dest.AIInsights, opt => opt.Ignore())
                .ForMember(dest => dest.LastCompletedAt, opt => opt.Ignore());

            // UpdateHabitDto -> Habit (for patching)
            CreateMap<UpdateHabitDto, Habit>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Goal <-> GoalDto
            CreateMap<Goal, GoalDto>().ReverseMap();

            // CreateGoalDto -> Goal
            CreateMap<CreateGoalDto, Goal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // UpdateGoalDto -> Goal (for patching)
            CreateMap<UpdateGoalDto, Goal>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
