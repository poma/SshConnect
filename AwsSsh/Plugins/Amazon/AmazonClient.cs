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
		public static List<AmazonInstance> GetInstances()
		{
			try
			{
				var result = new List<AmazonInstance>();

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

					var instance = new AmazonInstance()
					{
						Id = image.InstanceId,
						Name = name,
						StateName = image.InstanceState.Name,
						State = (AmazonInstatnceStates)image.InstanceState.Code,
						PublicIp = image.IpAddress,
						PrivateIp = image.PrivateIpAddress,
						InstanceType = image.InstanceType,
						PublicDnsName = image.PublicDnsName,
						PrivateDnsName = image.PrivateDnsName
					};
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
		public static void MergeInstanceList(ObservableCollection<Instance> existingInstances, List<AmazonInstance> newInstances)
		{
			var c = new AmazonInstanceComparer();
			//var itemsToRemove = existingInstances.Except(newInstances, c).Where(a => !a.IsPuttyInstance).ToList();
			var itemsToRemove = existingInstances.OfType<AmazonInstance>().Except(newInstances, c).OfType<AmazonInstance>().ToList();
			var itemsToAdd = newInstances.Except(existingInstances.OfType<AmazonInstance>(), c).ToList();
			var itemsToUpdate = existingInstances.OfType<AmazonInstance>().Join(newInstances, a => a.Id, a => a.Id, (a, b) => new { Old = a, New = b }).ToList();
			itemsToAdd.ForEach(a => existingInstances.Add(a));
			itemsToRemove.ForEach(a => existingInstances.Remove(a));
			//itemsToUpdate.ForEach(a => AmazonInstance.AssignInstance(a.Old, a.New));
			itemsToUpdate.ForEach(a =>
			{
				var ind = existingInstances.IndexOf(a.Old);
				existingInstances.Remove(a.Old);
				existingInstances.Insert(ind, a.New);
			});
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
