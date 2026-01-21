using Application.Topics.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Topics.Dtos;


public class TopicResponseDto
{
    public Guid Id { get; init; } // init вместо set для иммутабельности после создания
    public string Title { get; init; } = default!;
    public DateTime? EventStart { get; init; }
    public string Summary { get; init; } = default!;
    public string TopicType { get; init; } = default!;
    public LocationDto Location { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }

    public bool IsDeleted => DeletedAt.HasValue;
}
