using Application.DTOs.TaskDtos;

namespace Application.IService;

public interface ITaskService
{
    Task<TaskDto?> GetTaskByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskDto>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken = default);
    Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTaskAsync(Guid id, CancellationToken cancellationToken = default);
}
