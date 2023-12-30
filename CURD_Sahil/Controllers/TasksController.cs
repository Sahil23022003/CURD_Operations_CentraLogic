using Azure;
using CURD_Sahil.DTO;
using CURD_Sahil.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace CURD_Sahil.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        //Created an environment variable instead of this hard code in lauchSettings.json file

        //public string URI = "https://localhost:8081";
        //public string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        //public string DatabaseName = "TasksCurd";
        //public string ContainerName = "TaskBucket";

        private Container GetContainer()
        {
            string URI = Environment.GetEnvironmentVariable("Cosmos-Uri");
            string PrimaryKey = Environment.GetEnvironmentVariable("Primary-Key");
            string DatabaseName = Environment.GetEnvironmentVariable("Database");
            string ContainerName = Environment.GetEnvironmentVariable("Container");

            CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
            Database database = cosmosClient.GetDatabase(DatabaseName);
            Container container = database.GetContainer(ContainerName);
            return container;
        }


        public readonly Container _container;
        public TasksController()
        {
            _container = GetContainer();
        }


        [HttpPost]
        public async Task<IActionResult> AddTask(TasksModel tasksModel)
        {
            try
            {
                Tasks tasksEntity = new Tasks();

                //Mapping of users fields
                tasksEntity.TaskId = tasksModel.TaskId;
                tasksEntity.Title = tasksModel.Title;
                tasksEntity.Description = tasksModel.Description;

                //Mapping of mandatory fields
                tasksEntity.Id = Guid.NewGuid().ToString();
                tasksEntity.UId = tasksEntity.Id;
                tasksEntity.DocumentType = "task"; ;

                tasksEntity.CreatedOn = DateTime.Now;
                tasksEntity.CreatedByName = "Sahil";
                tasksEntity.CreatedBy = "Sahil's UId";

                tasksEntity.UpdatedOn = DateTime.Now;
                tasksEntity.UpdatedByName = "Virat";
                tasksEntity.UpdatedBy = "Kohli's UId";

                tasksEntity.Version = 1;
                tasksEntity.Active = true;
                tasksEntity.Archieved = false;

                Tasks response = await _container.CreateItemAsync(tasksEntity);

                //Reverse Mapping
                TasksModel model = new TasksModel();
                model.UId = response.UId;
                model.TaskId = response.TaskId;
                model.Title = response.Title;
                model.Description = response.Description;

                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest("Data Adding Failed" + ex);
            }
        }


        [HttpGet]
        public IActionResult GetTasksByUId(string uId)
        {
            try
            {
                Tasks tasks = _container.GetItemLinqQueryable<Tasks>(true)
                    .Where(b => b.DocumentType == "task" && b.UId == uId)
                    .AsEnumerable().FirstOrDefault();

                //Mapping
                var tasksModel = new TasksModel();

                tasksModel.UId = tasks.UId;
                tasksModel.TaskId = tasks.TaskId;
                tasksModel.Title = tasks.Title;
                tasksModel.Description = tasks.Description;
                return Ok(tasksModel);
            }
            catch (Exception ex)
            {
                return BadRequest("Getting Particular Task Failed" + ex);
            }
        }


        [HttpGet]
        public IActionResult GetAllTasks()
        {
            try
            {
                var tasksList = _container.GetItemLinqQueryable<Tasks>(true)
                    .Where(b => b.DocumentType == "task" && b.Archieved == false &&b.Active ==true)
                    .AsEnumerable().ToList();
                List<TasksModel> tasksModelList =new List<TasksModel>();
                foreach(var task in tasksList) 
                { 
                    TasksModel model = new TasksModel();
                    model.UId = task.UId;
                    model.TaskId = task.TaskId;
                    model.Title = task.Title;
                    model.Description = task.Description;

                    tasksModelList.Add(model);
                }

                return Ok(tasksModelList);
            }
            catch(Exception ex) 
            {
                return BadRequest("Getting All Tasks failed" + ex);
            }
        }



        [HttpPut]
        public async Task<IActionResult>UpdateTask(TasksModel tasksModel)
        {
            try
            {
                var existingTask = _container.GetItemLinqQueryable<Tasks>(true)
                    .Where(q => q.UId == tasksModel.UId && q.DocumentType == "task" && q.Archieved == false && q.Active == true)
                    .AsEnumerable().FirstOrDefault();
                existingTask.Archieved = true;
                await _container.ReplaceItemAsync(existingTask, existingTask.Id);

                existingTask.Id = Guid.NewGuid().ToString();
                existingTask.UpdatedBy = "";
                existingTask.UpdatedByName = "";
                existingTask.UpdatedOn = DateTime.Now;
                existingTask.Version = existingTask.Version + 1;
                existingTask.Active = true;
                existingTask.Archieved = false;


                existingTask.TaskId = tasksModel.TaskId;
                existingTask.Title = tasksModel.Title;
                existingTask.Description = tasksModel.Description;

                existingTask = await _container.CreateItemAsync(existingTask);

                TasksModel model = new TasksModel();
                model.UId = existingTask.UId;
                model.TaskId = existingTask.TaskId;
                model.Title = existingTask.Title;
                model.Description = existingTask.Description;

                return Ok(model);
            }
            catch(Exception ex)
            {
                return BadRequest("Task Updation Failed" + ex);
            }
        }
        [HttpDelete]
        public async Task<IActionResult>DeleteTask(string TaskUId)
        {
            try
            {
                var task = _container.GetItemLinqQueryable<Tasks>(true)
                    .Where(q => q.UId == TaskUId && q.DocumentType == "task" && q.Archieved == false && q.Active == true)
                    .AsEnumerable().FirstOrDefault();
                task.Active = false;
                await _container.ReplaceItemAsync(task, task.Id);
                return Ok(true);
            }
            catch(Exception ex)
            {
                return BadRequest("Data Deletion Failed" + ex);
            }
        }

    }
}
