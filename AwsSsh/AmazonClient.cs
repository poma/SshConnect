using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using AwsSsh.Properties;
using System.Windows;

namespace AwsSsh
{
	public static class AmazonClient
	{
		public static List<Instance> GetInstances()
		{
			try
			{
				var result = new List<Instance>();

				AmazonEC2 ec2 = AWSClientFactory.CreateAmazonEC2Client(
					App.Settings.AWSAccessKey,
					App.Settings.AWSSecretKey,
					new AmazonEC2Config { ServiceURL = App.Settings.ServiceUrl }
					);

				var ec2Response = ec2.DescribeInstances(new DescribeInstancesRequest());

				foreach (var image in ec2Response.DescribeInstancesResult.Reservation.SelectMany(a => a.RunningInstance))
				{
					var nameTag = image.Tag.Where(t => t.Key == "Name").FirstOrDefault();
					var name = nameTag != null ? nameTag.Value : image.InstanceId;

					var instance = new Instance()
					{
						Id = image.InstanceId,
						Name = name,
						StateName = image.InstanceState.Name,
						State = (InstatnceStates)image.InstanceState.Code,
						PublicIp = image.IpAddress,
						PrivateIp = image.PrivateIpAddress,
						InstanceType = image.InstanceType,
						PublicDnsName = image.PublicDnsName,
						PrivateDnsName = image.PrivateDnsName
					};
					if (!Enum.IsDefined(typeof(InstatnceStates), instance.State)) 
						instance.State = InstatnceStates.Unknown;
					result.Add(instance);
				}
				return result.OrderBy(a => a.Name).ToList();
			}
			catch (AmazonEC2Exception ex)
			{
				string message = ex.Message + "\r\n" +
				"Response Status Code: " + ex.StatusCode + "\r\n" +
				"Error Code: " + ex.ErrorCode + "\r\n" +
				"Error Type: " + ex.ErrorType + "\r\n" +
				"Request ID: " + ex.RequestId;
				throw new AmazonEC2Exception(message, ex);
			}
		}

		/// <summary>
		/// Used to merge new instance info but retain references
		/// </summary>
		public static void MergeInstanceList(ObservableCollection<Instance> existingInstances, List<Instance> newInstances)
		{
			var c = new InstanceComparer();
			var itemsToRemove = existingInstances.Except(newInstances, c).Where(a => !a.IsPuttyInstance).ToList();
			var itemsToAdd = newInstances.Except(existingInstances, c).ToList();
			var itemsToUpdate = existingInstances.Join(newInstances, a => a.Id, a => a.Id, (a, b) => new { Old = a, New = b }).ToList();
			itemsToAdd.ForEach(a => existingInstances.Add(a));
			itemsToRemove.ForEach(a => existingInstances.Remove(a));
			itemsToUpdate.ForEach(a => Instance.AssignInstance(a.Old, a.New));
		}

		public static bool CheckConnection()
		{

			try
			{
				AmazonEC2 ec2 = AWSClientFactory.CreateAmazonEC2Client(
						App.Settings.AWSAccessKey,
						App.Settings.AWSSecretKey,
						new AmazonEC2Config { ServiceURL = App.Settings.ServiceUrl }
						);

				ec2.DescribeInstances(new DescribeInstancesRequest());

				return true;
			}
			catch (AmazonEC2Exception ex)
			{
				string message = ex.Message + "\r\n" +
				"Response Status Code: " + ex.StatusCode + "\r\n" +
				"Error Code: " + ex.ErrorCode + "\r\n" +
				"Error Type: " + ex.ErrorType + "\r\n" +
				"Request ID: " + ex.RequestId;
				ExceptionDialog.Show(new AmazonEC2Exception(message, ex));
				return false;
			}
			catch (Exception ex)
			{
				ExceptionDialog.Show(ex);
				return false;
			}
		}
	}
}
