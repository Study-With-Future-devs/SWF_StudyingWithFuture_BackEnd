using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Studying_With_Future.Controllers.Base;
using Studying_With_Future.Data;
using Studying_With_Future.Models;

namespace Studying_With_Future.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisciplinasController : BaseController<Disciplina>
    {
        public DisciplinasController(AppDbContext context) : base(context) { }
    }
}