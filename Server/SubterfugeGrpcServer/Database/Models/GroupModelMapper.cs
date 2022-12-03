using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class GroupModelMapper : ProtoClassMapper<GroupModel>
    {
        public string Id;
        public List<String> GroupMembers;
        public string RoomId;

        public GroupModelMapper(GroupModel groupModel)
        {
            Id = groupModel.Id;
            GroupMembers = groupModel.GroupMembers.ToList();
            RoomId = groupModel.RoomId;
        }
        
        public override GroupModel ToProto()
        {
            return new GroupModel()
            {
                Id = Id,
                GroupMembers = { GroupMembers },
                RoomId = RoomId
            };
        }
    }
}