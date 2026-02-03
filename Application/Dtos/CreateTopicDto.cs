using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos;

// Record для запроса создания - иммутабельный
public record CreateTopicDto(
    string Title,
    string Summary,
    string TopicType,
    LocationDto Location,
    DateTime? EventStart
);
