using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using AwsSsh.Properties;
using System.Windows;

namespace AwsSsh.Plugins.Amazon
{
	public class AmazonInstanceSource: IInstanceSource
	{
		public List<Instance> GetInstanceList()
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
						Source = this,
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
				return result.OrderBy(a => a.Name).Cast<Instance>().ToList();
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
