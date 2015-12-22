using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ConnectForSharePoint.Test
{
    public class MemoryContentTypeRepository : ContentTypeRepository
    {
        private ContentTypeModelRepository _modelRepository;
        private List<ContentType> _contentTypes = new List<ContentType>();

        public MemoryContentTypeRepository()
            : this(new ContentTypeModelRepository())
        { }

        public MemoryContentTypeRepository(ContentTypeModelRepository modelRepository)
        {
            _modelRepository = modelRepository;
        }

        public override ContentType Load(Type modelType)
        {
            return MergeModelSettings(_contentTypes.FirstOrDefault(pt => pt.ModelType == modelType));
        }

        private ContentType MergeModelSettings(ContentType contentType)
        {
            if (contentType == null)
            {
                return null;
            }

            ContentTypeModel model = _modelRepository.GetContentTypeModel(contentType.ModelType);
            if (model != null)
            {
                ModelMerger merger = new ModelMerger(new Mock<IContentModelUsage>().Object);
                merger.MergeModelSettings(contentType, model);
            }
            return contentType;
        }

        public override ContentType Copy(ContentType contentType)
        {
            throw new NotImplementedException();
        }

        public override void Delete(ContentType contentType)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ContentType> List()
        {
            throw new NotImplementedException();
        }

        public override ContentType Load(Guid guid)
        {
            throw new NotImplementedException();
        }

        public override ContentType Load(string name)
        {
            throw new NotImplementedException();
        }

        public override ContentType Load(int id)
        {
            throw new NotImplementedException();
        }

        public override void Save(ContentType contentType)
        {
            throw new NotImplementedException();
        }
    }
}
