using EPiServer.Core;
using System;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    class FakeContentEvents : IContentEvents
    {
        public int LoadedContentCounter = 0;
        public int LoadedChildrenCounter = 0;

        public event ChildrenEventHandler LoadingChildren;
        public event ChildrenEventHandler LoadedChildren
        {
            add { LoadedChildrenCounter++; }
            remove { if (LoadedChildrenCounter != 0) { LoadedChildrenCounter--; } }
        }

        public event ChildrenEventHandler FailedLoadingChildren;
        public event EventHandler<ContentEventArgs> LoadingContent;

        public event EventHandler<ContentEventArgs> LoadedContent
        {
            add { LoadedContentCounter++; }
            remove { if (LoadedContentCounter != 0) { LoadedContentCounter--; } }
        }

        public event EventHandler<ContentEventArgs> FailedLoadingContent;
        public event EventHandler<ContentEventArgs> LoadingDefaultContent;
        public event EventHandler<ContentEventArgs> LoadedDefaultContent;
        public event EventHandler<ContentEventArgs> PublishingContent;
        public event EventHandler<ContentEventArgs> PublishedContent;
        public event EventHandler<ContentEventArgs> CheckingInContent;
        public event EventHandler<ContentEventArgs> CheckedInContent;
        public event EventHandler<ContentEventArgs> RequestingApproval;
        public event EventHandler<ContentEventArgs> RequestedApproval;
        public event EventHandler<ContentEventArgs> RejectingContent;
        public event EventHandler<ContentEventArgs> RejectedContent;
        public event EventHandler<ContentEventArgs> CheckingOutContent;
        public event EventHandler<ContentEventArgs> CheckedOutContent;
        public event EventHandler<ContentEventArgs> SchedulingContent;
        public event EventHandler<ContentEventArgs> ScheduledContent;
        public event EventHandler<DeleteContentEventArgs> DeletingContent;
        public event EventHandler<DeleteContentEventArgs> DeletedContent;
        public event EventHandler<ContentEventArgs> CreatingContentLanguage;
        public event EventHandler<ContentEventArgs> CreatedContentLanguage;
        public event EventHandler<ContentEventArgs> DeletingContentLanguage;
        public event EventHandler<ContentEventArgs> DeletedContentLanguage;
        public event EventHandler<ContentEventArgs> MovingContent;
        public event EventHandler<ContentEventArgs> MovedContent;
        public event EventHandler<ContentEventArgs> CreatingContent;
        public event EventHandler<ContentEventArgs> CreatedContent;
        public event EventHandler<ContentEventArgs> SavingContent;
        public event EventHandler<ContentEventArgs> SavedContent;
        public event EventHandler<ContentEventArgs> DeletingContentVersion;
        public event EventHandler<ContentEventArgs> DeletedContentVersion;
    }
}
