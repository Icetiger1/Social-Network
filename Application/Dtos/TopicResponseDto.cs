using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos;

public record TopicResponseDto(
    Guid Id,
    string Title,
    string Summary,
    string TopicType,
    string City,
    string Street,
    DateTime? EventStart,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt
    );

