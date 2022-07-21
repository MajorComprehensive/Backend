﻿using System;
using System.Collections.Generic;

namespace youAreWhatYouEat.Models
{
    public partial class CommentOnService
    {
        public string CommentId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime? CommentTime { get; set; }
        public decimal? Stars { get; set; }
        public string? CommentContent { get; set; }

        public virtual Vip UserNameNavigation { get; set; } = null!;
    }
}
