using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;

namespace Infra
{
    public class ApiStack : Stack
    {
        internal ApiStack(Construct scope, string id, ApiProps props = null) : base(scope, id, props)
        {
            var api = new ApplicationLoadBalancedFargateService(this, $"{props.ServiceName}-fargate-service", new ApplicationLoadBalancedFargateServiceProps
            {
                ServiceName = props.ServiceName,
                Cluster = props.EcsCluster,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    ContainerName = props.ServiceName,
                    Image = ContainerImage.FromEcrRepository(props.EcrRepository),
                    Environment = props.ContainerEnvVars,
                    Secrets = props.ContainerSecrets,
                    EnableLogging = true
                }
            });

            api.TargetGroup.ConfigureHealthCheck(new HealthCheck
            {
                Path = "/health"
            });

            //seems handy https://github.com/aws/aws-cdk/issues/8352
            //also handy https://chekkan.com/iam-policy-perm-for-public-load-balanced-ecs-fargate-on-cdk/
        }
    }
}
