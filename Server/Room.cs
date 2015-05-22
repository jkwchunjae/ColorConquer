using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using ColorConquer;

namespace Server
{
	class Room
	{
		User Alice, Bob;

		public Room()
		{
			Alice = null;
			Bob = null;
		}

		public bool EnterUser(User user)
		{
			if (Alice == null)
			{
				Alice = user;
			}
			else if (Bob == null)
			{
				Bob = user;
			}
			else
			{
				return false;
			}
			return true;
		}
	}

}
