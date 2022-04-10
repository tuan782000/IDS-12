using IDS_School.Data;
using IDS_School.Models;
using IDS_School.Service;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace IDS_School.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubmissionsController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public SubmissionsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _env = env;
            _context = context;
        }
        // GET: SubmissionsController
        public async Task<IActionResult> Index()
        {
            return View(await _context.Submissions.ToListAsync());
        }

        // GET: SubmissionsController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: Admin/Submissions/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var submission = await _context.Submissions
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (submission == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(submission);
        //}
        //public int pageSize = 2;

        // GET: SubmissionsController/Details/5
        public  IActionResult Details(int? id, int? pageNumber)
        {
            if (id == null)
            {
                return NotFound();
            }
            var getIdea = _context.Ideas.Include(i => i.Submission).Where(s => s.SubmissionId == id);
            //var data = (from s in _db.Posts select s);

            /*if (page > 0)
            {
                page = page;
            }
            else
            {
                page = 1;
            }
            int start = (int)(page - 1) * pageSize;

            ViewBag.pageCurrent = page;
            int totalPage = getIdea.Count();
            float totalNumsize = (totalPage / (float)pageSize);
            int numSize = (int)Math.Ceiling(totalNumsize);
            ViewBag.numSize = numSize;
            ViewBag.ideas = getIdea.OrderByDescending(x => x.Id).Skip(start).Take(pageSize);*/

            //var getIdea = _context.Ideas.Include(i => i.Submission).Where(s => s.SubmissionId == id);
            /*var submission = await _context.Submissions
                .FirstOrDefaultAsync(m => m.Id == id);*/

            ViewData["SubmissionId"] = id;
            int pageSize = 5;
            return View(PaginatedList<Idea>.Create(getIdea.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        // GET: Admin/Submissions/CreateIdea
        public IActionResult CreateIdea(int submissionId)
        {
            ViewData["SubmissionId"] = submissionId;
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: IdeasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIdea(int submissionId, IFormFile file, Idea idea, Boolean isAnoymous, bool isAcceptTerms = false)
        {
            if (isAcceptTerms)
            {
                //if (idea.CreatedDate <= idea.LastModifiedDate)
                //{ 
                
                //}
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                idea.UserId = user;

                Idea newIdea = new Idea
                {
                    UserId = user,
                    SubmissionId = submissionId,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    Title = idea.Title,
                    Description = idea.Description,
                    Content = idea.Content,
                    CategoryId = idea.CategoryId,
                    isAnoymous = isAnoymous,
                };

                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", idea.CategoryId);


                _context.Add(newIdea);
                await _context.SaveChangesAsync();

                FileType? fileType;
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                switch (fileExtension)
                {
                    case ".doc": case ".docx": fileType = FileType.Document; break;
                    case ".jpg": case ".png": fileType = FileType.img; break;
                    default: fileType = null; break;
                }

                if (fileType != null)
                {
                    string webRootPath = _env.WebRootPath;
                    var path = Path.Combine(webRootPath, Global.PATH_TOPIC, newIdea.SubmissionId.ToString());

                    if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                    // Upload file
                    path = Path.Combine(path, String.Format("{0}.{1:yyyy-MM-dd.ss-mm-HH}{2}", user, DateTime.Now, fileExtension));
                    using var stream = new FileStream(path, FileMode.Create);
                    file.CopyTo(stream);

                    // var newFile = new FileStream(path, FileMode.Create);
                    var newFile = new Models.File();
                    newFile.IdeaId = newIdea.Id;
                    newFile.FilePath = path;
                    newFile.CreatedDate = DateTime.Now;
                    newFile.LastModifiedDate = DateTime.Now;
                    newFile.Type = (FileType)fileType;

                    _context.Add(newFile);
                    await _context.SaveChangesAsync();
                }
                //using (var client = new SmtpClient())
                //{
                //    client.Connect("smtp.gmail.com");
                //    client.Authenticate("nguyenhaithaonhi3g@gmail.com", "znfndgnpbwzlhzeh");
                //    BodyBuilder bodyBuilder = new BodyBuilder();
                //    bodyBuilder.TextBody = $"Hello {newIdea.User.Email},\n\n" +
                //                           $"Your contribution for {newIdea.Title} is {newIdea.Category.Name}.\n\n" +
                //                           $"Thank you for your contribution,\n\n" +
                //                           $"Best regards,";


                //    var message = new MimeMessage
                //    {
                //        Body = bodyBuilder.ToMessageBody()
                //    };
                //    message.From.Add(new MailboxAddress("No reply my site", "nguyenhaithaonhi3g@gmail.com"));
                //    message.To.Add(new MailboxAddress("Test", idea.User.Email));
                //    message.Subject = "test";
                //    client.Send(message);

                //    client.Disconnect(true);
                //}

                ////tên của người nộp
                //var ideaFullname = $"{newIdea.User.FirstName} {newIdea.User.LastName}";
                //// Gửi mail
                //MailboxAddress from = new MailboxAddress("IDS_School System", "nguyenhaithaonhi3g@gmail.com");
                //MailboxAddress to = new MailboxAddress(ideaFullname, newIdea.User.Email);
                ////Gủi từ ai from hệ thống to người gửi mail nhận 
                //BodyBuilder bodyBuilder = new BodyBuilder();
                //bodyBuilder.TextBody = $"Hello {ideaFullname},\n\n" +
                //                       $"Your idea for {newIdea.User.Ideas}.\n\n" +
                //                       $"Thank you for your idea,\n\n" +
                //                       $"Best regards,";
                ////gửi
                //MimeMessage message = new MimeMessage();
                //message.From.Add(from);
                //message.To.Add(to);
                //message.Subject = $"Idea for {newIdea.User.Ideas}";
                //message.Body = bodyBuilder.ToMessageBody();

                //SmtpClient client = new SmtpClient();
                //client.Connect("smtp.gmail.com", 587, true);
                //client.Authenticate("nguyenhaithaonhi3g@gmail.com", "znfndgnpbwzlhzeh");

                //client.Send(message);
                //client.Disconnect(true);
                //client.Dispose();
            }
            else
            {
                return RedirectToAction(nameof(Details), new SelectList(_context.Submissions, "Id", "Id", idea.SubmissionId));
            }

            return View(idea);
        }
        //GET: Admin/Submissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Submissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Submission submission)
        {
            if (ModelState.IsValid)
            {
                if (submission.ClosureDate <= submission.FinalClosureDate)
                {
                    //submission.CreationDay = DateTime.Now;

                    _context.Add(submission);
                    await _context.SaveChangesAsync();

                    var folderName = submission.Id.ToString();

                    var path = Path.Combine(Global.PATH_TOPIC, folderName);

                    if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

                    return RedirectToAction(nameof(Index));
                }
                ViewData["Error"] = "Final Closure Date is not acceptable.";
            }

            return View(submission);
        }

        // GET: Admin/Submissions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            return View(submission);
        }

        // POST: Admin/Submissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ClosureDate,FinalClosureDate,CreationDay")] Submission submission)
        {
            if (id != submission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (submission.ClosureDate <= submission.FinalClosureDate)
                {
                    try
                    {
                        _context.Update(submission);
                        await _context.SaveChangesAsync();

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SubmissionExists(submission.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                ViewData["Error"] = "Final Closure Date is not acceptable.";
            }
            return View(submission);
        }

        // GET: Admin/Submissions/Edit/5
        public async Task<IActionResult> EditIdea(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            return View(submission);
        }

        // POST: Admin/Submissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIdea(int id, [Bind("Id,Name,Description,ClosureDate,FinalClosureDate,CreationDay")] Submission submission)
        {
            if (id != submission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (submission.ClosureDate <= submission.FinalClosureDate)
                {
                    try
                    {
                        _context.Update(submission);
                        await _context.SaveChangesAsync();

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SubmissionExists(submission.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                ViewData["Error"] = "Final Closure Date is not acceptable.";
            }
            return View(submission);
        }

        // GET: Admin/Submissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        // POST: SubmissionsController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();

            var folderName = id.ToString();

            var path = Path.Combine(Global.PATH_TOPIC, folderName);

            if (Directory.Exists(path)) { Directory.Delete(path); }

            return RedirectToAction(nameof(Index));
        }

        private bool SubmissionExists(int id)
        {
            return _context.Submissions.Any(e => e.Id == id);
        }

        // GET: IdeasController/Details/5
        public async Task<IActionResult> DetailIdea(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idea = await _context.Ideas
                .Include(r => r.Reactions)
                .Include(c => c.Comments)
                .Include(f => f.Files)
                .FirstOrDefaultAsync(m => m.Id == id);

            var getReact = await _context.Reactions
               .Where(t => t.IdeaId == id && t.reaction == reaction.Like)
               .Select(t => t.reaction == reaction.Like)
               .ToListAsync();

            var getReact2 = await _context.Reactions
              .Where(t => t.IdeaId == id && t.reaction == reaction.Dislike)
              .Select(t => t.reaction == reaction.Dislike)
              .ToListAsync();
            int countLike = getReact.Count;
            int countDislike = getReact2.Count;

            List<Comment> comments = null;
            comments = await _context.Comments
                .Include(u => u.User)
                 .Include(u => u.Replies)
                .Where(i => i.IdeaId == id)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            List<Reply> replies = null;
            replies = await _context.Replies
                .Include(u => u.User)
                .Include(u => u.Comment)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            ViewData["countLike"] = countLike;
            ViewData["countDislike"] = countDislike;
            ViewData["comments"] = comments;
            ViewData["replies"] = replies;
            //ViewData["files"] = files;





            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }
        // POST: IdeasController/Like/10
        [HttpPost, ActionName("Like")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int ideaId, reaction getReaction)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var idea = await _context.Ideas.FindAsync(ideaId);
            var oldArticle = (from x in _context.Reactions
                              where x.UserId == user
                              select x).FirstOrDefault();
            if (oldArticle == null && getReaction == reaction.Like)
            {
                Reaction newReaction = new Reaction
                {
                    UserId = user,
                    IdeaId = ideaId,
                    CreatedDate = DateTime.Now,
                    reaction = Models.reaction.Like
                };

                _context.Add(newReaction);
                await _context.SaveChangesAsync();

            }
            else if (oldArticle == null && getReaction == reaction.Dislike)
            {
                Reaction newReaction = new Reaction
                {
                    UserId = user,
                    IdeaId = ideaId,
                    CreatedDate = DateTime.Now,
                    reaction = Models.reaction.Dislike
                };
                _context.Add(newReaction);
                await _context.SaveChangesAsync();
            }
            else if (oldArticle != null && oldArticle.reaction != getReaction)
            {
                oldArticle.reaction = getReaction;
                _context.Update(oldArticle);
                await _context.SaveChangesAsync();
            }
            else if (oldArticle != null && oldArticle.reaction == getReaction)
            {
                _context.Remove(oldArticle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DetailIdea), new { id = ideaId });
        }

        // POST: IdeasController/AddComment/12
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(string message, int id, Boolean isAnoymous)
        {

            var idea = await _context.Ideas.FindAsync(id);
            int ideaid = idea.Id;
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!String.IsNullOrEmpty(message))
            {
                var newComment = new Comment();
                newComment.Content = message;
                newComment.IdeaId = ideaid;
                newComment.UserId = user;
                newComment.CreatedDate = DateTime.Now;
                newComment.LastModifiedDate = DateTime.Now;
                newComment.isAnoymous = isAnoymous;

                _context.Add(newComment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DetailIdea), new { id });
        }
        // POST: IdeasController/Reply/13
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(string replyMessage, int ideaId, Boolean isAnoymous, int CommentId)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!String.IsNullOrEmpty(replyMessage))
            {
                var newReplyComment = new Reply();
                newReplyComment.Content = replyMessage;
                newReplyComment.CommentId = CommentId;
                newReplyComment.UserId = user;
                newReplyComment.CreatedDate = DateTime.Now;
                newReplyComment.LastModifiedDate = DateTime.Now;
                newReplyComment.isAnoymous = isAnoymous;

                _context.Add(newReplyComment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DetailIdea), new { id = ideaId });
        }
    }
}
