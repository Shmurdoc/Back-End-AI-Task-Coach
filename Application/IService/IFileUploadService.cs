﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, Guid userId);
    }
}
