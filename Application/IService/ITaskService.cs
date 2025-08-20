using Application.DTOs.TaskDtos;

namespace Application.IService;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken cancellationToken);
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken);
    Task<IEnumerable<TaskDto>> GetUserTasksAsync(Guid userId);
}
