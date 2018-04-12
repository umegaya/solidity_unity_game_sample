FROM hashicorp/terraform:light

RUN apk add python

# Install the Google Cloud SDK. 
ENV HOME / 
ENV CLOUDSDK_PYTHON_SITEPACKAGES 1 
RUN wget https://dl.google.com/dl/cloudsdk/channels/rapid/google-cloud-sdk.zip && unzip google-cloud-sdk.zip && rm google-cloud-sdk.zip
RUN google-cloud-sdk/install.sh --usage-reporting=true --path-update=true --bash-completion=true --rc-path=/.bashrc --additional-components app-engine-java app-engine-python app kubectl alpha beta gcd-emulator pubsub-emulator cloud-datastore-emulator app-engine-go bigtable

# Disable updater check for the whole installation. # Users won't be bugged with notifications to update to the latest version of gcloud. 
RUN google-cloud-sdk/bin/gcloud config set --installation component_manager/disable_update_check true

# Disable updater completely. # Running `gcloud components update` doesn't really do anything in a union FS. # Changes are lost on a subsequent run. 
RUN sed -i -- 's/\"disable_updater\": false/\"disable_updater\": true/g' /google-cloud-sdk/lib/googlecloudsdk/core/config.json

RUN mkdir /.ssh
RUN mkdir -p /.terraform.d/plugins/linux_amd64 && wget https://github.com/sl1pm4t/terraform-provider-kubernetes/releases/download/v1.0.8-custom/terraform-provider-kubernetes_v1.0.8-custom_linux_amd64.zip && unzip terraform-provider-kubernetes_v1.0.8-custom_linux_amd64.zip && rm terraform-provider-kubernetes_v1.0.8-custom_linux_amd64.zip && mv terraform-provider-kubernetes_v1.0.8-custom_x4 /.terraform.d/plugins/linux_amd64

RUN ln -s /google-cloud-sdk /usr/lib

ENV PATH /google-cloud-sdk/bin:/bin:$PATH
VOLUME ["/.config"]
